using Learnify.Api.Common;
using Learnify.Application.Common.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;

namespace Learnify.Api.Filters;

/// <summary>
/// Action filter that automatically wraps all controller responses in ApiResponse<T>
/// </summary>
public class ApiResponseActionFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Nothing to do before action executes
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Skip if already handled by exception middleware
        if (context.Exception != null)
            return;

        // Skip if response is already set (e.g., file downloads)
        if (context.HttpContext.Response.HasStarted)
            return;

        var result = context.Result;

        // Handle different result types
        switch (result)
        {
            case ObjectResult objectResult when ShouldWrap(objectResult):
                context.Result = WrapObjectResult(objectResult);
                break;

            case StatusCodeResult statusCodeResult:
                context.Result = WrapStatusCodeResult(statusCodeResult);
                break;

            case EmptyResult:
                context.Result = WrapEmptyResult(context);
                break;
        }
    }

    /// <summary>
    /// Determines if the response should be wrapped
    /// </summary>
    private bool ShouldWrap(ObjectResult objectResult)
    {
        // Don't wrap if already an ApiResponse
        if (objectResult.Value?.GetType().Name.StartsWith("ApiResponse") == true)
            return false;

        // Don't wrap problem details
        if (objectResult.Value is ProblemDetails)
            return false;

        // Don't wrap file results
        if (objectResult.Value is FileResult || objectResult.Value is FileContentResult)
            return false;

        return true;
    }

    /// <summary>
    /// Wraps ObjectResult in ApiResponse
    /// </summary>
    private IActionResult WrapObjectResult(ObjectResult objectResult)
    {
        var statusCode = objectResult.StatusCode ?? 200;
        var value = objectResult.Value;

        // Check if it's a paginated result
        if (IsPagedResult(value, out var pagedData, out var paginationInfo))
        {
            var message = GetSuccessMessage(statusCode);
            var response = new
            {
                success = true,
                statusCode = statusCode,
                message = message,
                data = pagedData,
                pagination = paginationInfo,
                metadata = new ResponseMetadata()
            };

            return new ObjectResult(response) { StatusCode = statusCode };
        }

        // Regular response
        if (statusCode >= 200 && statusCode < 300)
        {
            var message = GetSuccessMessage(statusCode);
            var response = new
            {
                success = true,
                statusCode = statusCode,
                message = message,
                data = value,
                metadata = new ResponseMetadata()
            };

            return new ObjectResult(response) { StatusCode = statusCode };
        }
        else
        {
            // Error response
            var message = GetErrorMessage(statusCode, value);
            var response = new
            {
                success = false,
                statusCode = statusCode,
                message = message,
                errors = ExtractErrors(value),
                metadata = new ResponseMetadata()
            };

            return new ObjectResult(response) { StatusCode = statusCode };
        }
    }

    /// <summary>
    /// Wraps StatusCodeResult in ApiResponse
    /// </summary>
    private IActionResult WrapStatusCodeResult(StatusCodeResult statusCodeResult)
    {
        var statusCode = statusCodeResult.StatusCode;

        if (statusCode >= 200 && statusCode < 300)
        {
            var response = new
            {
                success = true,
                statusCode = statusCode,
                message = GetSuccessMessage(statusCode),
                data = (object?)null,
                metadata = new ResponseMetadata()
            };

            return new ObjectResult(response) { StatusCode = statusCode };
        }
        else
        {
            var response = new
            {
                success = false,
                statusCode = statusCode,
                message = GetErrorMessage(statusCode, null),
                errors = (object?)null,
                metadata = new ResponseMetadata()
            };

            return new ObjectResult(response) { StatusCode = statusCode };
        }
    }

    /// <summary>
    /// Wraps EmptyResult in ApiResponse
    /// </summary>
    private IActionResult WrapEmptyResult(ActionExecutedContext context)
    {
        var statusCode = context.HttpContext.Response.StatusCode;

        if (statusCode >= 200 && statusCode < 300)
        {
            var response = new
            {
                success = true,
                statusCode = statusCode,
                message = GetSuccessMessage(statusCode),
                data = (object?)null,
                metadata = new ResponseMetadata()
            };

            return new ObjectResult(response) { StatusCode = statusCode };
        }
        else
        {
            var response = new
            {
                success = false,
                statusCode = statusCode,
                message = GetErrorMessage(statusCode, null),
                errors = (object?)null,
                metadata = new ResponseMetadata()
            };

            return new ObjectResult(response) { StatusCode = statusCode };
        }
    }

    /// <summary>
    /// Checks if value is a PagedResult and extracts pagination info
    /// </summary>
    private bool IsPagedResult(object? value, out object? data, out object? pagination)
    {
        data = null;
        pagination = null;

        if (value == null)
            return false;

        var type = value.GetType();

        // Check if it's PagedResult<T>
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(PagedResult<>))
        {
            var itemsProperty = type.GetProperty("Items");
            var currentPageProperty = type.GetProperty("CurrentPage");
            var pageSizeProperty = type.GetProperty("PageSize");
            var totalPagesProperty = type.GetProperty("TotalPages");
            var totalCountProperty = type.GetProperty("TotalCount");

            if (itemsProperty != null && currentPageProperty != null)
            {
                data = itemsProperty.GetValue(value);
                pagination = new PaginationMetadata
                {
                    CurrentPage = (int)(currentPageProperty.GetValue(value) ?? 1),
                    PageSize = (int)(pageSizeProperty?.GetValue(value) ?? 10),
                    TotalPages = (int)(totalPagesProperty?.GetValue(value) ?? 1),
                    TotalCount = (int)(totalCountProperty?.GetValue(value) ?? 0)
                };
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Extracts error details from response value
    /// </summary>
    private object? ExtractErrors(object? value)
    {
        if (value == null)
            return null;

        // Check if value has a Message property
        var messageProperty = value.GetType().GetProperty("Message");
        if (messageProperty != null)
        {
            var message = messageProperty.GetValue(value)?.ToString();
            if (!string.IsNullOrEmpty(message))
            {
                return new[]
                {
                    new ErrorDetail
                    {
                        Code = "ERROR",
                        Message = message
                    }
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Gets a success message based on status code
    /// </summary>
    private string GetSuccessMessage(int statusCode)
    {
        return statusCode switch
        {
            200 => "Operation completed successfully",
            201 => "Resource created successfully",
            202 => "Request accepted for processing",
            204 => "Operation completed successfully",
            _ => "Success"
        };
    }

    /// <summary>
    /// Gets an error message based on status code
    /// </summary>
    private string GetErrorMessage(int statusCode, object? value)
    {
        // Try to extract message from value
        if (value != null)
        {
            var messageProperty = value.GetType().GetProperty("Message");
            var message = messageProperty?.GetValue(value)?.ToString();
            if (!string.IsNullOrEmpty(message))
                return message;
        }

        // Default messages based on status code
        return statusCode switch
        {
            400 => "Bad request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Resource not found",
            409 => "Conflict",
            422 => "Validation failed",
            429 => "Too many requests",
            500 => "Internal server error",
            503 => "Service unavailable",
            _ => "An error occurred"
        };
    }
}
