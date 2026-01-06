using System.Threading;

namespace Learnify.Api.Logging;

/// <summary>
/// Thread-safe static class for tracking API request metrics in-memory.
/// Useful for monitoring and diagnostics without external dependencies.
/// </summary>
public static class ApiMetrics
{
    private static long _totalRequests;
    private static long _successfulRequests;
    private static long _failedRequests;
    private static long _totalDurationMs;

    /// <summary>
    /// Records a completed request with its status code and duration.
    /// Thread-safe for concurrent access.
    /// </summary>
    /// <param name="statusCode">HTTP status code of the response</param>
    /// <param name="durationMs">Request duration in milliseconds</param>
    public static void RecordRequest(int statusCode, long durationMs)
    {
        Interlocked.Increment(ref _totalRequests);
        Interlocked.Add(ref _totalDurationMs, durationMs);

        if (statusCode < 400)
            Interlocked.Increment(ref _successfulRequests);
        else
            Interlocked.Increment(ref _failedRequests);
    }

    /// <summary>
    /// Gets a snapshot of the current metrics.
    /// </summary>
    public static ApiMetricsSnapshot GetSnapshot()
    {
        var total = Interlocked.Read(ref _totalRequests);
        return new ApiMetricsSnapshot
        {
            TotalRequests = total,
            SuccessfulRequests = Interlocked.Read(ref _successfulRequests),
            FailedRequests = Interlocked.Read(ref _failedRequests),
            AverageDurationMs = total > 0
                ? Interlocked.Read(ref _totalDurationMs) / (double)total
                : 0
        };
    }

    /// <summary>
    /// Resets all metrics to zero. Useful for testing or periodic resets.
    /// </summary>
    public static void Reset()
    {
        Interlocked.Exchange(ref _totalRequests, 0);
        Interlocked.Exchange(ref _successfulRequests, 0);
        Interlocked.Exchange(ref _failedRequests, 0);
        Interlocked.Exchange(ref _totalDurationMs, 0);
    }
}

/// <summary>
/// Immutable snapshot of API metrics at a point in time.
/// </summary>
public class ApiMetricsSnapshot
{
    public long TotalRequests { get; init; }
    public long SuccessfulRequests { get; init; }
    public long FailedRequests { get; init; }
    public double AverageDurationMs { get; init; }

    /// <summary>
    /// Success rate as a percentage (0-100).
    /// </summary>
    public double SuccessRate => TotalRequests > 0
        ? (double)SuccessfulRequests / TotalRequests * 100
        : 0;

    /// <summary>
    /// Failure rate as a percentage (0-100).
    /// </summary>
    public double FailureRate => TotalRequests > 0
        ? (double)FailedRequests / TotalRequests * 100
        : 0;
}
