using System.Threading.Tasks;

namespace Learnify.Application.BackgroundJobs.Interfaces;

/// <summary>
/// Interface for cleanup-related background jobs.
/// These jobs run on a recurring schedule to maintain system hygiene.
/// </summary>
public interface ICleanupJobService
{
    /// <summary>
    /// Remove expired OTP codes from the database.
    /// Should run hourly.
    /// </summary>
    Task CleanupExpiredOtpsAsync();

    /// <summary>
    /// Remove abandoned shopping carts that haven't been updated in a while.
    /// Should run daily.
    /// </summary>
    Task CleanupAbandonedCartsAsync();

    /// <summary>
    /// Clean up old log entries or temporary files.
    /// Should run weekly.
    /// </summary>
    Task CleanupOldTempFilesAsync();
}
