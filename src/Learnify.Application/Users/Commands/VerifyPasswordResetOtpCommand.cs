using MediatR;

namespace Learnify.Application.Users.Commands;

/// <summary>
/// Command to verify password reset OTP and get reset token
/// </summary>
public class VerifyPasswordResetOtpCommand : IRequest<VerifyPasswordResetOtpResult>
{
    public required string Email { get; set; }
    public required string Otp { get; set; }
}

public class VerifyPasswordResetOtpResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ResetToken { get; set; } // Short-lived token for setting new password
}
