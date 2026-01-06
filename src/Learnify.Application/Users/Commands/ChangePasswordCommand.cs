using MediatR;

namespace Learnify.Application.Users.Commands;

/// <summary>
/// Command to change password when logged in (requires old password)
/// </summary>
public class ChangePasswordCommand : IRequest<ChangePasswordResult>
{
    public required string UserId { get; set; }
    public required string OldPassword { get; set; }
    public required string NewPassword { get; set; }
}

public class ChangePasswordResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}
