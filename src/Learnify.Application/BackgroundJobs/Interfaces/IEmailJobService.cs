using System.Collections.Generic;
using System.Threading.Tasks;

namespace Learnify.Application.BackgroundJobs.Interfaces;

/// <summary>
/// Interface for email-related background jobs.
/// Implementations should be idempotent and handle retries gracefully.
/// </summary>
public interface IEmailJobService
{
    /// <summary>
    /// Send OTP verification email as a background job.
    /// </summary>
    Task SendOtpEmailAsync(string to, string userName, string otp, string purpose);

    /// <summary>
    /// Send payment confirmation email as a background job.
    /// </summary>
    Task SendPaymentConfirmationAsync(string to, string userName, decimal amount, List<string> courseNames);

    /// <summary>
    /// Send welcome email to new users as a background job.
    /// </summary>
    Task SendWelcomeEmailAsync(string to, string userName);

    /// <summary>
    /// Send course completion email with certificate link as a background job.
    /// </summary>
    Task SendCourseCompletionEmailAsync(string to, string userName, string courseName, string certificateUrl);

    /// <summary>
    /// Send payment submitted notification as a background job.
    /// </summary>
    Task SendPaymentSubmittedEmailAsync(string to, string userName, decimal amount, string paymentMethod, List<string> courseNames);

    /// <summary>
    /// Send payment rejected notification as a background job.
    /// </summary>
    Task SendPaymentRejectedEmailAsync(string to, string userName, decimal amount, string? adminNote);
}
