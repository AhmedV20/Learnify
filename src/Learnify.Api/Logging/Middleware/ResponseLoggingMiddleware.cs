using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Learnify.Api.Logging.Middleware;

/// <summary>
/// Middleware for logging response bodies for debugging purposes.
/// - In Development: Logs all responses at Debug level
/// - In All Environments: Logs error responses (status >= 400) at Warning level
/// Implements sensitive data masking and response truncation.
/// </summary>
public class ResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ResponseLoggingMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    private const int MaxResponseLength = 4000;

    private static readonly HashSet<string> LoggableContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/json",
        "application/problem+json",
        "text/plain",
        "text/html"
    };

    /// <summary>
    /// Fields that should be masked in response logs for security.
    /// </summary>
    private static readonly HashSet<string> SensitiveFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "password", "secret", "token", "apikey", "api_key", "accessToken", "access_token",
        "refreshToken", "refresh_token", "creditcard", "credit_card", "cardnumber",
        "card_number", "cvv", "otp", "ssn", "social_security", "pin", "passcode",
        "private_key", "privatekey", "authorization"
    };

    public ResponseLoggingMiddleware(
        RequestDelegate next,
        ILogger<ResponseLoggingMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip response logging for non-API requests (static files, health checks, etc.)
        if (ShouldSkipLogging(context))
        {
            await _next(context);
            return;
        }

        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);

            if (ShouldLogResponse(context))
            {
                await LogResponseAsync(context, responseBody);
            }
        }
        finally
        {
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }

    private bool ShouldSkipLogging(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";

        // Skip static files, health checks, and swagger
        return path.StartsWith("/health") ||
               path.StartsWith("/swagger") ||
               path.StartsWith("/scalar") ||
               path.StartsWith("/hangfire") ||
               path.EndsWith(".js") ||
               path.EndsWith(".css") ||
               path.EndsWith(".map") ||
               path.EndsWith(".ico");
    }

    private bool ShouldLogResponse(HttpContext context)
    {
        // Always log error responses in any environment
        if (context.Response.StatusCode >= 400)
            return true;

        // In Development, log all API responses
        return _env.IsDevelopment();
    }

    private async Task LogResponseAsync(HttpContext context, MemoryStream responseBody)
    {
        var contentType = context.Response.ContentType ?? "";

        // Only log text-based content types
        if (!LoggableContentTypes.Any(t => contentType.Contains(t, StringComparison.OrdinalIgnoreCase)))
            return;

        responseBody.Seek(0, SeekOrigin.Begin);
        var responseText = await new StreamReader(responseBody, Encoding.UTF8).ReadToEndAsync();
        responseBody.Seek(0, SeekOrigin.Begin);

        // Skip empty responses
        if (string.IsNullOrWhiteSpace(responseText))
            return;

        // Mask sensitive data
        responseText = MaskSensitiveData(responseText);

        // Truncate large responses
        var isTruncated = false;
        if (responseText.Length > MaxResponseLength)
        {
            responseText = responseText[..MaxResponseLength];
            isTruncated = true;
        }

        var logLevel = context.Response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Debug;
        var responseSize = responseBody.Length;

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["ResponseStatusCode"] = context.Response.StatusCode,
            ["ResponseContentType"] = contentType,
            ["ResponseBodySize"] = responseSize,
            ["IsTruncated"] = isTruncated
        }))
        {
            _logger.Log(logLevel,
                "Response {RequestId}: Status={StatusCode}, Size={ResponseSize}B, Body={ResponseBody}{Truncated}",
                context.TraceIdentifier,
                context.Response.StatusCode,
                responseSize,
                responseText,
                isTruncated ? " [TRUNCATED]" : "");
        }
    }

    /// <summary>
    /// Masks sensitive fields in JSON data to prevent logging credentials.
    /// </summary>
    private static string MaskSensitiveData(string data)
    {
        if (string.IsNullOrEmpty(data))
            return data;

        foreach (var field in SensitiveFields)
        {
            // Regex pattern to match "fieldName": "value" or "fieldName":"value"
            var pattern = $@"(""{field}""\s*:\s*)""[^""]*""";
            data = Regex.Replace(data, pattern, "$1\"***MASKED***\"", RegexOptions.IgnoreCase);

            // Also handle numeric values like "pin": 1234
            var numericPattern = $@"(""{field}""\s*:\s*)\d+";
            data = Regex.Replace(data, numericPattern, "$1***", RegexOptions.IgnoreCase);
        }

        return data;
    }
}
