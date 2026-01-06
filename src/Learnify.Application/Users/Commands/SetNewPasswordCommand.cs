using MediatR;

namespace Learnify.Application.Users.Commands;

/// <summary>
/// Command to set new password using reset token
/// </summary>
public class SetNewPasswordCommand : IRequest<SetNewPasswordResult>
{
    public required string Email { get; set; }
    public required string ResetToken { get; set; }
    public required string NewPassword { get; set; }
}

public class SetNewPasswordResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}
