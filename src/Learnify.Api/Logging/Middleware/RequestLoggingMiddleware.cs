using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Learnify.Api.Logging.Middleware;

/// <summary>
/// Middleware for comprehensive request/response logging with sensitive data masking.
/// Logs method, path, status code, duration, and masked request body for debugging.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    /// <summary>
    /// Headers that should never be logged (contain sensitive data)
    /// </summary>
    private static readonly HashSet<string> SensitiveHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization", "Cookie", "X-API-Key", "X-Auth-Token", "Set-Cookie"
    };

    /// <summary>
    /// JSON field names that should be masked in logs
    /// </summary>
    private static readonly HashSet<string> SensitiveFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "password", "secret", "token", "apikey", "api_key", "creditcard", "credit_card",
        "cardnumber", "card_number", "cvv", "otp", "ssn", "social_security",
        "pin", "passcode", "private_key", "privatekey"
    };

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = context.TraceIdentifier;

        // Log request start
        await LogRequestAsync(context, requestId);

        // Capture response body for logging
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            // Log response
            LogResponse(context, requestId, stopwatch.ElapsedMilliseconds);

            // Copy the response body back to the original stream
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }

    private async Task LogRequestAsync(HttpContext context, string requestId)
    {
        var request = context.Request;

        // Only log body for specific content types and methods
        string? requestBody = null;
        if (ShouldLogRequestBody(request))
        {
            request.EnableBuffering();
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            request.Body.Position = 0;

            // Mask sensitive data before logging
            requestBody = MaskSensitiveData(requestBody);

            // Truncate very large bodies
            if (requestBody.Length > 2000)
            {
                requestBody = requestBody[..2000] + "... [TRUNCATED]";
            }
        }

        _logger.LogInformation(
            "HTTP {Method} {Path} started | RequestId: {RequestId} | QueryString: {QueryString} | ContentLength: {ContentLength} | ContentType: {ContentType}",
            request.Method,
            request.Path.Value,
            requestId,
            request.QueryString.Value ?? "(none)",
            request.ContentLength ?? 0,
            request.ContentType ?? "(none)");

        // Log body separately at Debug level (only in Development)
        if (!string.IsNullOrEmpty(requestBody))
        {
            _logger.LogDebug(
                "Request body for {RequestId}: {RequestBody}",
                requestId,
                requestBody);
        }
    }

    private void LogResponse(HttpContext context, string requestId, long elapsedMs)
    {
        var statusCode = context.Response.StatusCode;

        // Choose log level based on status code
        var logLevel = statusCode switch
        {
            >= 500 => LogLevel.Error,
            >= 400 => LogLevel.Warning,
            _ => LogLevel.Information
        };

        // Add performance warning for slow requests
        if (elapsedMs > 3000)
        {
            _logger.LogWarning(
                "SLOW REQUEST: HTTP {Method} {Path} took {ElapsedMs}ms | Status: {StatusCode} | RequestId: {RequestId}",
                context.Request.Method,
                context.Request.Path.Value,
                elapsedMs,
                statusCode,
                requestId);
        }
        else
        {
            _logger.Log(
                logLevel,
                "HTTP {Method} {Path} completed | Status: {StatusCode} | Duration: {ElapsedMs}ms | RequestId: {RequestId}",
                context.Request.Method,
                context.Request.Path.Value,
                statusCode,
                elapsedMs,
                requestId);
        }
    }

    /// <summary>
    /// Determines if the request body should be logged based on method and content type
    /// </summary>
    private static bool ShouldLogRequestBody(HttpRequest request)
    {
        var method = request.Method;
        if (method != HttpMethods.Post && method != HttpMethods.Put && method != HttpMethods.Patch)
            return false;

        var contentType = request.ContentType ?? "";
        return contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase)
            || contentType.Contains("application/xml", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Masks sensitive fields in JSON data to prevent logging credentials
    /// </summary>
    private static string MaskSensitiveData(string data)
    {
        if (string.IsNullOrEmpty(data))
            return data;

        foreach (var field in SensitiveFields)
        {
            // Regex pattern to match "fieldName": "value" or "fieldName":"value"
            // Handles both quoted strings and numbers
            var pattern = $@"(""{field}""\s*:\s*)""[^""]*""";
            data = Regex.Replace(data, pattern, "$1\"***MASKED***\"", RegexOptions.IgnoreCase);

            // Also handle numeric values like "pin": 1234
            var numericPattern = $@"(""{field}""\s*:\s*)\d+";
            data = Regex.Replace(data, numericPattern, "$1***", RegexOptions.IgnoreCase);
        }

        return data;
    }
}
