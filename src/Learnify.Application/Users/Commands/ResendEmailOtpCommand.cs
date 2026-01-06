using MediatR;

namespace Learnify.Application.Users.Commands;

/// <summary>
/// Command to resend email verification OTP
/// </summary>
public class ResendEmailOtpCommand : IRequest<ResendEmailOtpResult>
{
    public required string Email { get; set; }
}

public class ResendEmailOtpResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}
