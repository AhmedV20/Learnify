namespace Learnify.Domain.Enums;

/// <summary>
/// Two-Factor Authentication methods available for users
/// </summary>
public enum TwoFactorMethod
{
    /// <summary>
    /// OTP code sent via email
    /// </summary>
    Email = 0,
    
    /// <summary>
    /// TOTP code from authenticator app (Google Authenticator, Authy, etc.)
    /// </summary>
    Authenticator = 1
}
