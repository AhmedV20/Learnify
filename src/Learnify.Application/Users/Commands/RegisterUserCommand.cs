using MediatR;

namespace Learnify.Application.Users.Commands;

/// <summary>
/// Command to register a new user
/// </summary>
public class RegisterUserCommand : IRequest<RegisterResponse>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
}

/// <summary>
/// Response for registration attempts
/// </summary>
public class RegisterResponse
{
    /// <summary>
    /// Status of the registration attempt
    /// </summary>
    public AuthResponseStatus Status { get; set; } = AuthResponseStatus.Success;
    
    /// <summary>
    /// Human-readable message about the result
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// User ID (only set on successful registration or if email exists but unverified)
    /// </summary>
    public string? UserId { get; set; }
    
    /// <summary>
    /// Email address (for frontend to use in OTP verification)
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// True if user needs to verify OTP before proceeding
    /// </summary>
    public bool RequiresVerification { get; set; }
}