using Learnify.Application.Users.Commands;
using MediatR;

namespace Learnify.Application.Features.Authentication.Commands;

public class LoginCommand : IRequest<LoginResponse>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Response for login attempts with status codes for different scenarios
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// Status of the login attempt
    /// </summary>
    public AuthResponseStatus Status { get; set; } = AuthResponseStatus.Success;
    
    /// <summary>
    /// Human-readable message about the result
    /// </summary>
    public string? Message { get; set; }
    
    /// <summary>
    /// True if user needs to verify OTP before proceeding
    /// </summary>
    public bool RequiresVerification { get; set; }
    
    /// <summary>
    /// Email address (for OTP verification flow)
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// JWT access token (only set on successful login)
    /// </summary>
    public string? Token { get; set; }
    
    /// <summary>
    /// Token expiration date/time
    /// </summary>
    public DateTime? Expiration { get; set; }
    
    /// <summary>
    /// User information (only set on successful login)
    /// </summary>
    public UserDto? UserData { get; set; }
    
    /// <summary>
    /// True if 2FA verification is required
    /// </summary>
    public bool Requires2FA { get; set; }
    
    /// <summary>
    /// Temporary token for 2FA verification (valid 5 minutes)
    /// </summary>
    public string? TwoFactorToken { get; set; }
    
    /// <summary>
    /// The 2FA method the user has configured
    /// </summary>
    public Learnify.Domain.Enums.TwoFactorMethod? TwoFactorMethod { get; set; }
}

/// <summary>
/// User data returned with successful login
/// </summary>
public record UserDto
{
    public string Id { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? SecondName { get; set; }
    public string? Email { get; set; }
    public string? ProfileImageUrl { get; set; }
    public List<string> Roles { get; set; } = new();
}

// Keep for backward compatibility with other endpoints
public class TokenResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
}