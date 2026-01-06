using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Learnify.Api.Logging.Middleware;

/// <summary>
/// Middleware for performance monitoring with configurable thresholds.
/// Categorizes requests as NORMAL, SLOW, or VERY_SLOW and logs accordingly.
/// Also records metrics to ApiMetrics for aggregate tracking.
/// </summary>
public class PerformanceLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceLoggingMiddleware> _logger;
    private readonly PerformanceLoggingOptions _options;

    public PerformanceLoggingMiddleware(
        RequestDelegate next,
        ILogger<PerformanceLoggingMiddleware> logger,
        IOptions<PerformanceLoggingOptions> options)
    {
        _next = next;
        _logger = logger;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = context.GetEndpoint()?.DisplayName ?? "Unknown";

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            // Record metrics for aggregate tracking
            ApiMetrics.RecordRequest(context.Response.StatusCode, elapsedMs);

            // Log performance with categorization
            LogPerformanceMetrics(context, endpoint, elapsedMs);
        }
    }

    private void LogPerformanceMetrics(HttpContext context, string endpoint, long elapsedMs)
    {
        var performanceCategory = CategorizePerformance(elapsedMs);
        var logLevel = GetLogLevel(performanceCategory);

        // Only log SLOW and VERY_SLOW requests to reduce noise
        // NORMAL requests are tracked in metrics but not individually logged
        if (performanceCategory == "NORMAL")
            return;

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["PerformanceCategory"] = performanceCategory,
            ["Endpoint"] = endpoint,
            ["Method"] = context.Request.Method,
            ["Path"] = context.Request.Path.Value ?? "",
            ["StatusCode"] = context.Response.StatusCode,
            ["DurationMs"] = elapsedMs,
            ["ResponseContentLength"] = context.Response.ContentLength ?? 0,
            ["RequestContentLength"] = context.Request.ContentLength ?? 0
        }))
        {
            _logger.Log(logLevel,
                "[{PerformanceCategory}] {Method} {Path} completed in {DurationMs}ms (Status: {StatusCode})",
                performanceCategory,
                context.Request.Method,
                context.Request.Path.Value,
                elapsedMs,
                context.Response.StatusCode);
        }
    }

    private string CategorizePerformance(long elapsedMs)
    {
        return elapsedMs switch
        {
            _ when elapsedMs > _options.VerySlowThresholdMs => "VERY_SLOW",
            _ when elapsedMs > _options.SlowThresholdMs => "SLOW",
            _ => "NORMAL"
        };
    }

    private static LogLevel GetLogLevel(string category)
    {
        return category switch
        {
            "VERY_SLOW" => LogLevel.Warning,
            "SLOW" => LogLevel.Information,
            _ => LogLevel.Debug
        };
    }
}

/// <summary>
/// Configuration options for performance logging thresholds.
/// Configure in appsettings.json under "PerformanceLogging" section.
/// </summary>
public class PerformanceLoggingOptions
{
    public const string SectionName = "PerformanceLogging";

    /// <summary>
    /// Threshold in milliseconds for a request to be considered "slow".
    /// Default: 500ms
    /// </summary>
    public int SlowThresholdMs { get; set; } = 500;

    /// <summary>
    /// Threshold in milliseconds for a request to be considered "very slow".
    /// Default: 2000ms
    /// </summary>
    public int VerySlowThresholdMs { get; set; } = 2000;
}
