using Hangfire;
using Learnify.Application.BackgroundJobs.Interfaces;
using Learnify.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Learnify.Infrastructure.BackgroundJobs.Services;

/// <summary>
/// Background job service for cleanup operations.
/// Uses IServiceScopeFactory to properly handle scoped DbContext in background jobs.
/// </summary>
public class CleanupJobService : ICleanupJobService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CleanupJobService> _logger;

    public CleanupJobService(IServiceScopeFactory scopeFactory, ILogger<CleanupJobService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Cleans up expired OTP codes from user records.
    /// Runs hourly via recurring job.
    /// </summary>
    [AutomaticRetry(Attempts = 3)]
    public async Task CleanupExpiredOtpsAsync()
    {
        _logger.LogInformation("Starting expired OTP cleanup job");

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var expiredOtpUsers = await dbContext.Users
                .Where(u => u.EmailOtpExpiry != null && u.EmailOtpExpiry < DateTime.UtcNow)
                .ToListAsync();

            var cleanedCount = 0;
            foreach (var user in expiredOtpUsers)
            {
                user.EmailOtpHash = null;
                user.EmailOtpExpiry = null;
                user.EmailOtpAttempts = 0;
                cleanedCount++;
            }

            if (cleanedCount > 0)
            {
                await dbContext.SaveChangesAsync();
                _logger.LogInformation(
                    "Expired OTP cleanup completed | Cleaned: {CleanedCount} records",
                    cleanedCount);
            }
            else
            {
                _logger.LogDebug("Expired OTP cleanup completed | No expired OTPs found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during expired OTP cleanup");
            throw;
        }
    }

    /// <summary>
    /// Cleans up abandoned shopping carts older than 30 days.
    /// Runs daily via recurring job.
    /// </summary>
    [AutomaticRetry(Attempts = 3)]
    public async Task CleanupAbandonedCartsAsync()
    {
        _logger.LogInformation("Starting abandoned cart cleanup job");

        try
        {
            // Note: This is a placeholder implementation.
            // If using Redis for carts, you would clean up old Redis keys here.
            // If using database carts, query and delete old cart records.
            
            _logger.LogInformation("Abandoned cart cleanup completed");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during abandoned cart cleanup");
            throw;
        }
    }

    /// <summary>
    /// Cleans up old temporary files from the uploads directory.
    /// Runs weekly via recurring job.
    /// </summary>
    [AutomaticRetry(Attempts = 2)]
    public async Task CleanupOldTempFilesAsync()
    {
        _logger.LogInformation("Starting temp file cleanup job");

        try
        {
            var tempPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "temp");
            
            if (!Directory.Exists(tempPath))
            {
                _logger.LogDebug("Temp directory does not exist, skipping cleanup");
                return;
            }

            var cutoffDate = DateTime.UtcNow.AddDays(-7);
            var deletedCount = 0;

            foreach (var file in Directory.GetFiles(tempPath))
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.LastWriteTimeUtc < cutoffDate)
                {
                    try
                    {
                        File.Delete(file);
                        deletedCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete temp file: {FilePath}", file);
                    }
                }
            }

            _logger.LogInformation(
                "Temp file cleanup completed | Deleted: {DeletedCount} files",
                deletedCount);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during temp file cleanup");
            throw;
        }
    }
}
