using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Learnify.Application.BackgroundJobs.Interfaces;

/// <summary>
/// Abstraction for background job scheduling.
/// This interface hides the Hangfire implementation details from the Application layer.
/// </summary>
public interface IBackgroundJobService
{
    /// <summary>
    /// Enqueue a fire-and-forget job to be executed as soon as possible.
    /// </summary>
    /// <typeparam name="T">The service type containing the method to execute</typeparam>
    /// <param name="methodCall">Expression representing the method to call</param>
    /// <returns>Job ID for tracking</returns>
    string Enqueue<T>(Expression<Func<T, Task>> methodCall);

    /// <summary>
    /// Schedule a delayed job to be executed after a specified time.
    /// </summary>
    /// <typeparam name="T">The service type containing the method to execute</typeparam>
    /// <param name="methodCall">Expression representing the method to call</param>
    /// <param name="delay">Time to wait before executing</param>
    /// <returns>Job ID for tracking</returns>
    string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay);

    /// <summary>
    /// Schedule a delayed job to be executed at a specific time.
    /// </summary>
    /// <typeparam name="T">The service type containing the method to execute</typeparam>
    /// <param name="methodCall">Expression representing the method to call</param>
    /// <param name="enqueueAt">The time to execute the job</param>
    /// <returns>Job ID for tracking</returns>
    string Schedule<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset enqueueAt);

    /// <summary>
    /// Create a continuation job that runs after a parent job completes.
    /// </summary>
    /// <typeparam name="T">The service type containing the method to execute</typeparam>
    /// <param name="parentJobId">ID of the job to wait for</param>
    /// <param name="methodCall">Expression representing the method to call</param>
    /// <returns>Job ID for tracking</returns>
    string ContinueWith<T>(string parentJobId, Expression<Func<T, Task>> methodCall);

    /// <summary>
    /// Add or update a recurring job using a cron expression.
    /// </summary>
    /// <typeparam name="T">The service type containing the method to execute</typeparam>
    /// <param name="recurringJobId">Unique identifier for the recurring job</param>
    /// <param name="methodCall">Expression representing the method to call</param>
    /// <param name="cronExpression">Cron schedule expression</param>
    void AddOrUpdateRecurring<T>(string recurringJobId, Expression<Func<T, Task>> methodCall, string cronExpression);

    /// <summary>
    /// Remove a recurring job.
    /// </summary>
    /// <param name="recurringJobId">ID of the recurring job to remove</param>
    void RemoveRecurring(string recurringJobId);
}
