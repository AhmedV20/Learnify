using Hangfire.Common;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace Learnify.Infrastructure.BackgroundJobs.Filters;

/// <summary>
/// Hangfire job filter that logs job execution lifecycle.
/// Applied globally to all jobs.
/// </summary>
public class LoggingJobFilter : JobFilterAttribute, IServerFilter
{
    private readonly ILogger<LoggingJobFilter> _logger;
    private static readonly string StartTimeKey = "JobStartTime";

    public LoggingJobFilter(ILogger<LoggingJobFilter> logger)
    {
        _logger = logger;
    }

    public void OnPerforming(PerformingContext context)
    {
        // Store start time for duration calculation
        context.Items[StartTimeKey] = Stopwatch.StartNew();

        _logger.LogInformation(
            "Job starting | JobId: {JobId} | Type: {JobType} | Method: {Method}",
            context.BackgroundJob.Id,
            context.BackgroundJob.Job.Type.Name,
            context.BackgroundJob.Job.Method.Name);
    }

    public void OnPerformed(PerformedContext context)
    {
        var duration = TimeSpan.Zero;
        if (context.Items.TryGetValue(StartTimeKey, out var startTimeObj) && startTimeObj is Stopwatch sw)
        {
            sw.Stop();
            duration = sw.Elapsed;
        }

        if (context.Exception != null)
        {
            _logger.LogError(
                context.Exception,
                "Job failed | JobId: {JobId} | Type: {JobType} | Duration: {DurationMs}ms | Error: {ErrorMessage}",
                context.BackgroundJob.Id,
                context.BackgroundJob.Job.Type.Name,
                duration.TotalMilliseconds,
                context.Exception.Message);
        }
        else
        {
            _logger.LogInformation(
                "Job completed | JobId: {JobId} | Type: {JobType} | Duration: {DurationMs}ms",
                context.BackgroundJob.Id,
                context.BackgroundJob.Job.Type.Name,
                duration.TotalMilliseconds);
        }
    }
}
