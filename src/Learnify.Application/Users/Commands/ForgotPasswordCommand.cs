using MediatR;

namespace Learnify.Application.Users.Commands;

/// <summary>
/// Command to request password reset OTP
/// </summary>
public class ForgotPasswordCommand : IRequest<ForgotPasswordResult>
{
    public required string Email { get; set; }
}

public class ForgotPasswordResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}
