using MediatR;

namespace Learnify.Application.Users.Commands;

/// <summary>
/// Command to verify email using OTP
/// </summary>
public class VerifyEmailOtpCommand : IRequest<VerifyEmailOtpResult>
{
    public required string Email { get; set; }
    public required string Otp { get; set; }
}

public class VerifyEmailOtpResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? Token { get; set; }
    public DateTime? Expiration { get; set; }
}
