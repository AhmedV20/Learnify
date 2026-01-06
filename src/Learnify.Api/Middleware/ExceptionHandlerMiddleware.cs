using Learnify.Api.Common;
using Learnify.Application.Common.Exceptions;
using Learnify.Application.Common.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Learnify.Api.Middleware
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception has occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = HttpStatusCode.InternalServerError;
            string message;
            List<ErrorDetail>? errors = null;

            switch (exception)
            {
                case ValidationException validationException:
                    statusCode = HttpStatusCode.BadRequest;
                    message = "One or more validation errors occurred.";
                    var allErrors = new List<ErrorDetail>();
                    
                    foreach (var failure in validationException.Failures)
                    {
                        // Extract description from CustomState if available
                        string description;
                        if (failure.CustomState is ValidationDescriptionState descState)
                        {
                            description = descState.GetDescription(failure.AttemptedValue);
                        }
                        else
                        {
                            description = $"The {failure.PropertyName.ToLower()} field failed validation";
                        }

                        allErrors.Add(new ErrorDetail
                        {
                            Code = "VALIDATION_ERROR",
                            Field = failure.PropertyName,
                            Message = failure.ErrorMessage,
                            Description = description
                        });
                    }
                    
                    // Deduplicate errors with same Field + Message combination
                    errors = allErrors
                        .DistinctBy(e => (e.Field, e.Message))
                        .ToList();
                    break;


                case NotFoundException notFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    message = notFoundException.Message;
                    errors = new List<ErrorDetail>
                    {
                        new ErrorDetail
                        {
                            Code = "NOT_FOUND",
                            Description = "The requested resource could not be found",
                            Message = notFoundException.Message
                        }
                    };
                    break;

                case UnauthorizedAccessException unauthorizedAccessException:
                    statusCode = HttpStatusCode.Unauthorized;
                    message = "Authentication failed.";
                    errors = new List<ErrorDetail>
                    {
                        new ErrorDetail
                        {
                            Code = "UNAUTHORIZED",
                            Description = "You are not authorized to access this resource",
                            Message = unauthorizedAccessException.Message
                        }
                    };
                    break;

                case DbUpdateException dbUpdateEx:
                    statusCode = HttpStatusCode.Conflict;
                    message = "A database conflict occurred.";
                    errors = new List<ErrorDetail>
                    {
                        new ErrorDetail
                        {
                            Code = "DATABASE_CONFLICT",
                            Description = "A conflict occurred while saving data",
                            Message = "The operation could not be completed due to a data conflict."
                        }
                    };
                    break;

                case OperationCanceledException:
                    statusCode = HttpStatusCode.RequestTimeout;
                    message = "The request was cancelled or timed out.";
                    errors = new List<ErrorDetail>
                    {
                        new ErrorDetail
                        {
                            Code = "REQUEST_TIMEOUT",
                            Description = "The operation timed out or was cancelled",
                            Message = "The request took too long to process."
                        }
                    };
                    break;

                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    message = "An unexpected error occurred. Please try again later.";
                    errors = new List<ErrorDetail>
                    {
                        new ErrorDetail
                        {
                            Code = "INTERNAL_ERROR",
                            Description = "An unexpected error occurred on the server",
                            Message = "An internal server error has occurred."
                        }
                    };
                    break;
            }

            // Create standardized error response
            var response = new
            {
                success = false,
                statusCode = (int)statusCode,
                message = message,
                errors = errors,
                metadata = new ResponseMetadata
                {
                    Timestamp = DateTime.UtcNow,
                    RequestId = context.TraceIdentifier,
                    CorrelationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? context.TraceIdentifier,
                    ApiVersion = "v1"
                }
            };

            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(response, jsonOptions);
            await context.Response.WriteAsync(json);
        }
    }
}
