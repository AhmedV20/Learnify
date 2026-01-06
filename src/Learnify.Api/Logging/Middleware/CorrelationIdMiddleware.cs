using Microsoft.AspNetCore.Http;
using Serilog.Context;
using System;
using System.Threading.Tasks;

namespace Learnify.Api.Logging.Middleware;

/// <summary>
/// Middleware that ensures each request has a unique correlation ID for tracing.
/// If X-Correlation-ID header is present, uses that value; otherwise generates a new one.
/// </summary>
public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Get or create correlation ID
        var correlationId = GetOrCreateCorrelationId(context);

        // Store in HttpContext for easy access
        context.Items["CorrelationId"] = correlationId;

        // Add to response headers so clients can track requests
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeader] = correlationId;
            return Task.CompletedTask;
        });

        // Add to Serilog's log context - all logs in this request will have this property
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        // Check if client sent a correlation ID
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId)
            && !string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId.ToString();
        }

        // Generate a short, readable correlation ID (12 chars from GUID)
        return Guid.NewGuid().ToString("N")[..12];
    }
}
