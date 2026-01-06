namespace Learnify.Application.TwoFactorAuth;

/// <summary>
/// Service for TOTP (Time-based One-Time Password) operations
/// Used for authenticator app 2FA (Google Authenticator, Authy, etc.)
/// </summary>
public interface ITotpService
{
    /// <summary>
    /// Generates a cryptographically secure secret key for TOTP
    /// </summary>
    /// <returns>Base32-encoded secret key</returns>
    string GenerateSecretKey();
    
    /// <summary>
    /// Generates a QR code URI for authenticator apps
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="secretKey">Base32-encoded secret key</param>
    /// <param name="issuer">App name shown in authenticator</param>
    /// <returns>otpauth:// URI for QR code</returns>
    string GenerateQrCodeUri(string email, string secretKey, string issuer = "Learnify");
    
    /// <summary>
    /// Generates a QR code image as base64 PNG
    /// </summary>
    /// <param name="qrCodeUri">The otpauth:// URI</param>
    /// <returns>Base64-encoded PNG image</returns>
    string GenerateQrCodeImage(string qrCodeUri);
    
    /// <summary>
    /// Validates a TOTP code against the secret key
    /// </summary>
    /// <param name="secretKey">Base32-encoded secret key</param>
    /// <param name="code">6-digit code from user</param>
    /// <returns>True if code is valid</returns>
    bool ValidateCode(string secretKey, string code);
}
