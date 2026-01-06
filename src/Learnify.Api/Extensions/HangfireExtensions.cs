// ⚠️ IMPORTANT: Before first run, take database backup!
// Hangfire will create ~12 tables in 'hangfire' schema
// Backup command: 
// BACKUP DATABASE [LearnifyDB] TO DISK = 'C:\Backups\LearnifyDB_BeforeHangfire.bak'

using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Learnify.Application.BackgroundJobs.Interfaces;
using Learnify.Infrastructure.BackgroundJobs.Filters;
using Learnify.Infrastructure.BackgroundJobs.Security;
using Learnify.Infrastructure.BackgroundJobs.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Learnify.Api.Extensions;

/// <summary>
/// Extension methods for configuring Hangfire background job processing.
/// </summary>
public static class HangfireExtensions
{
    /// <summary>
    /// Adds Hangfire services and configures SQL Server storage.
    /// </summary>
    public static IServiceCollection AddHangfireServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Hangfire services
        services.AddHangfire((serviceProvider, config) =>
        {
            config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(
                    configuration.GetConnectionString("DefaultConnection"),
                    new SqlServerStorageOptions
                    {
                        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                        QueuePollInterval = TimeSpan.Zero,
                        UseRecommendedIsolationLevel = true,
                        DisableGlobalLocks = true,
                        SchemaName = "hangfire",         // Separate schema for organization
                        PrepareSchemaIfNecessary = true,
                        
                        // Database growth management
                        JobExpirationCheckInterval = TimeSpan.FromHours(1),
                        DeleteExpiredBatchSize = 1000
                    });
        });

        // Configure global retry policy - delete jobs after max attempts to prevent indefinite retries
        GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute
        {
            Attempts = 3,
            OnAttemptsExceeded = AttemptsExceededAction.Delete
        });

        // Add the processing server with conservative worker count
        services.AddHangfireServer(options =>
        {
            // Conservative worker count - not ProcessorCount * 2
            options.WorkerCount = Math.Max(Environment.ProcessorCount, 2);
            options.Queues = new[] { "critical", "default", "low" };
            options.ServerTimeout = TimeSpan.FromMinutes(5);
            options.ShutdownTimeout = TimeSpan.FromSeconds(30);
        });

        // Register job services
        services.AddScoped<IBackgroundJobService, HangfireBackgroundJobService>();
        services.AddScoped<IEmailJobService, EmailJobService>();
        services.AddScoped<ICleanupJobService, CleanupJobService>();

        return services;
    }

    /// <summary>
    /// Configures Hangfire dashboard with security and registers recurring jobs.
    /// </summary>
    public static WebApplication UseHangfireDashboard(this WebApplication app)
    {
        // Configure dashboard with security
        var dashboardOptions = new DashboardOptions
        {
            Authorization = new[]
            {
                new HangfireDashboardAuthorizationFilter()
            },
            DashboardTitle = "Learnify Background Jobs",
            DisplayStorageConnectionString = false,
            IsReadOnlyFunc = context => !context.GetHttpContext().User.IsInRole("Admin")
        };

        app.MapHangfireDashboard("/hangfire", dashboardOptions);

        // Register recurring jobs
        RegisterRecurringJobs();

        return app;
    }

    /// <summary>
    /// Registers all recurring background jobs.
    /// </summary>
    private static void RegisterRecurringJobs()
    {
        // Cleanup expired OTPs every hour
        RecurringJob.AddOrUpdate<ICleanupJobService>(
            "cleanup-expired-otps",
            service => service.CleanupExpiredOtpsAsync(),
            Cron.Hourly);

        // Cleanup abandoned carts daily at 3 AM
        RecurringJob.AddOrUpdate<ICleanupJobService>(
            "cleanup-abandoned-carts",
            service => service.CleanupAbandonedCartsAsync(),
            Cron.Daily(3));

        // Cleanup old temp files weekly on Sunday at 4 AM
        RecurringJob.AddOrUpdate<ICleanupJobService>(
            "cleanup-temp-files",
            service => service.CleanupOldTempFilesAsync(),
            Cron.Weekly(DayOfWeek.Sunday, 4));
    }
}
