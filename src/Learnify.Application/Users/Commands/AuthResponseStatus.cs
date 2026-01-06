namespace Learnify.Application.Users.Commands;

/// <summary>
/// Status codes for authentication responses
/// </summary>
public enum AuthResponseStatus
{
    /// <summary>
    /// Operation completed successfully
    /// </summary>
    Success = 0,
    
    /// <summary>
    /// Invalid email or password
    /// </summary>
    InvalidCredentials = 1,
    
    /// <summary>
    /// Email exists but is not verified - OTP resent
    /// </summary>
    EmailNotVerified = 2,
    
    /// <summary>
    /// Account has been deactivated by admin
    /// </summary>
    AccountDeactivated = 3,
    
    /// <summary>
    /// Account is temporarily locked due to too many failed attempts
    /// </summary>
    AccountLocked = 4,
    
    /// <summary>
    /// Email already exists and is verified
    /// </summary>
    EmailAlreadyExists = 5,
    
    /// <summary>
    /// Credentials valid but 2FA verification is required
    /// </summary>
    Requires2FA = 6,
    
    /// <summary>
    /// User account has been banned/suspended
    /// </summary>
    UserBanned = 7,
    
    /// <summary>
    /// Email verification is required before proceeding
    /// </summary>
    RequiresVerification = 8,
    
    /// <summary>
    /// Generic server error
    /// </summary>
    Error = 99
}
