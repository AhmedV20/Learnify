using Hangfire;
using Learnify.Application.BackgroundJobs.Interfaces;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Learnify.Infrastructure.BackgroundJobs.Services;

/// <summary>
/// Hangfire implementation of the background job service.
/// Wraps Hangfire's static methods behind the abstraction for testability.
/// </summary>
public class HangfireBackgroundJobService : IBackgroundJobService
{
    public string Enqueue<T>(Expression<Func<T, Task>> methodCall)
    {
        return BackgroundJob.Enqueue(methodCall);
    }

    public string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay)
    {
        return BackgroundJob.Schedule(methodCall, delay);
    }

    public string Schedule<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset enqueueAt)
    {
        return BackgroundJob.Schedule(methodCall, enqueueAt);
    }

    public string ContinueWith<T>(string parentJobId, Expression<Func<T, Task>> methodCall)
    {
        return BackgroundJob.ContinueJobWith(parentJobId, methodCall);
    }

    public void AddOrUpdateRecurring<T>(string recurringJobId, Expression<Func<T, Task>> methodCall, string cronExpression)
    {
        RecurringJob.AddOrUpdate(recurringJobId, methodCall, cronExpression);
    }

    public void RemoveRecurring(string recurringJobId)
    {
        RecurringJob.RemoveIfExists(recurringJobId);
    }
}
