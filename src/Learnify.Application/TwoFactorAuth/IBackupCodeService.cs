namespace Learnify.Application.TwoFactorAuth;

/// <summary>
/// Service for generating and validating backup recovery codes
/// </summary>
public interface IBackupCodeService
{
    /// <summary>
    /// Generates a list of backup recovery codes
    /// </summary>
    /// <param name="count">Number of codes to generate (default 10)</param>
    /// <returns>List of plaintext backup codes (to show user once)</returns>
    List<string> GenerateBackupCodes(int count = 10);
    
    /// <summary>
    /// Hashes a backup code for secure storage
    /// </summary>
    /// <param name="code">Plaintext backup code</param>
    /// <returns>SHA256 hash of the code</returns>
    string HashBackupCode(string code);
    
    /// <summary>
    /// Validates a backup code against stored hashes
    /// </summary>
    /// <param name="code">Plaintext code from user</param>
    /// <param name="hashedCodesJson">JSON array of hashed codes from database</param>
    /// <param name="updatedHashedCodesJson">Updated JSON with used code removed</param>
    /// <returns>True if code is valid and was found</returns>
    bool ValidateAndConsumeBackupCode(string code, string hashedCodesJson, out string updatedHashedCodesJson);
}
