using Hangfire;
using Learnify.Application.BackgroundJobs.Interfaces;
using Learnify.Application.Email;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Learnify.Infrastructure.BackgroundJobs.Services;

/// <summary>
/// Background job service for sending emails.
/// All methods are designed to be idempotent and retry-safe.
/// </summary>
public class EmailJobService : IEmailJobService
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailJobService> _logger;

    public EmailJobService(IEmailService emailService, ILogger<EmailJobService> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Send OTP verification email with automatic retry on failure.
    /// Retries: 5 attempts with delays of 1min, 5min, 15min
    /// </summary>
    [AutomaticRetry(Attempts = 5, DelaysInSeconds = new[] { 60, 300, 900 })]
    public async Task SendOtpEmailAsync(string to, string userName, string otp, string purpose)
    {
        _logger.LogInformation(
            "Processing OTP email job | To: {Email} | Purpose: {Purpose}",
            MaskEmail(to), purpose);

        try
        {
            await _emailService.SendOtpEmailAsync(to, userName, otp, purpose);

            _logger.LogInformation(
                "OTP email sent successfully | To: {Email} | Purpose: {Purpose}",
                MaskEmail(to), purpose);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send OTP email | To: {Email} | Purpose: {Purpose} | Error: {ErrorMessage}",
                MaskEmail(to), purpose, ex.Message);

            // Re-throw to trigger Hangfire retry mechanism
            throw;
        }
    }

    /// <summary>
    /// Send payment confirmation email with automatic retry.
    /// </summary>
    [AutomaticRetry(Attempts = 5, DelaysInSeconds = new[] { 60, 300, 900 })]
    public async Task SendPaymentConfirmationAsync(string to, string userName, decimal amount, List<string> courseNames)
    {
        _logger.LogInformation(
            "Processing payment confirmation email | To: {Email} | Amount: {Amount} | Courses: {CourseCount}",
            MaskEmail(to), amount, courseNames.Count);

        try
        {
            await _emailService.SendPaymentApprovedEmailAsync(to, userName, amount, courseNames, null);

            _logger.LogInformation(
                "Payment confirmation email sent | To: {Email} | Amount: {Amount}",
                MaskEmail(to), amount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send payment confirmation email | To: {Email} | Error: {ErrorMessage}",
                MaskEmail(to), ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Send welcome email to new users.
    /// </summary>
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300 })]
    public async Task SendWelcomeEmailAsync(string to, string userName)
    {
        _logger.LogInformation(
            "Processing welcome email | To: {Email}",
            MaskEmail(to));

        try
        {
            await _emailService.SendWelcomeEmailAsync(to, userName);

            _logger.LogInformation("Welcome email sent | To: {Email}", MaskEmail(to));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email | To: {Email}", MaskEmail(to));
            throw;
        }
    }

    /// <summary>
    /// Send course completion email with certificate link.
    /// </summary>
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300 })]
    public async Task SendCourseCompletionEmailAsync(string to, string userName, string courseName, string certificateUrl)
    {
        _logger.LogInformation(
            "Processing course completion email | To: {Email} | Course: {CourseName}",
            MaskEmail(to), courseName);

        try
        {
            var subject = $"Congratulations! You've completed {courseName} 🎉";
            var body = $@"
Hello {userName},

Congratulations on completing '{courseName}'!

You can download your certificate of completion here:
{certificateUrl}

Keep up the great work and continue your learning journey!

Best regards,
The Learnify Team";

            await _emailService.SendEmailAsync(to, subject, body);

            _logger.LogInformation(
                "Course completion email sent | To: {Email} | Course: {CourseName}",
                MaskEmail(to), courseName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send course completion email | To: {Email} | Course: {CourseName}",
                MaskEmail(to), courseName);
            throw;
        }
    }

    /// <summary>
    /// Send payment submitted notification.
    /// </summary>
    [AutomaticRetry(Attempts = 5, DelaysInSeconds = new[] { 60, 300, 900 })]
    public async Task SendPaymentSubmittedEmailAsync(string to, string userName, decimal amount, string paymentMethod, List<string> courseNames)
    {
        _logger.LogInformation(
            "Processing payment submitted email | To: {Email} | Amount: {Amount}",
            MaskEmail(to), amount);

        try
        {
            await _emailService.SendPaymentSubmittedEmailAsync(to, userName, amount, paymentMethod, courseNames);

            _logger.LogInformation(
                "Payment submitted email sent | To: {Email} | Amount: {Amount}",
                MaskEmail(to), amount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send payment submitted email | To: {Email}",
                MaskEmail(to));
            throw;
        }
    }

    /// <summary>
    /// Send payment rejected notification.
    /// </summary>
    [AutomaticRetry(Attempts = 5, DelaysInSeconds = new[] { 60, 300, 900 })]
    public async Task SendPaymentRejectedEmailAsync(string to, string userName, decimal amount, string? adminNote)
    {
        _logger.LogInformation(
            "Processing payment rejected email | To: {Email} | Amount: {Amount}",
            MaskEmail(to), amount);

        try
        {
            await _emailService.SendPaymentRejectedEmailAsync(to, userName, amount, adminNote);

            _logger.LogInformation(
                "Payment rejected email sent | To: {Email} | Amount: {Amount}",
                MaskEmail(to), amount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send payment rejected email | To: {Email}",
                MaskEmail(to));
            throw;
        }
    }

    /// <summary>
    /// Masks email for logging to protect PII.
    /// </summary>
    private static string MaskEmail(string email)
    {
        if (string.IsNullOrEmpty(email) || !email.Contains('@'))
            return "***";

        var parts = email.Split('@');
        var localPart = parts[0];
        var domain = parts[1];

        if (localPart.Length <= 2)
            return $"**@{domain}";

        return $"{localPart[0]}***{localPart[^1]}@{domain}";
    }
}
