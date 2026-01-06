using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Learnify.Application.TwoFactorAuth;

namespace Learnify.Infrastructure.TwoFactorAuth;

/// <summary>
/// Backup code service implementation for recovery codes
/// </summary>
public class BackupCodeService : IBackupCodeService
{
    private const int CodeLength = 8;
    private const string CodeCharacters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // No I, O, 0, 1 to avoid confusion
    
    /// <inheritdoc />
    public List<string> GenerateBackupCodes(int count = 10)
    {
        var codes = new List<string>();
        
        for (int i = 0; i < count; i++)
        {
            codes.Add(GenerateSingleCode());
        }
        
        return codes;
    }
    
    /// <inheritdoc />
    public string HashBackupCode(string code)
    {
        // Normalize the code (uppercase, remove spaces/dashes)
        var normalizedCode = code.ToUpperInvariant().Replace("-", "").Replace(" ", "");
        
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(normalizedCode);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
    
    /// <inheritdoc />
    public bool ValidateAndConsumeBackupCode(string code, string hashedCodesJson, out string updatedHashedCodesJson)
    {
        updatedHashedCodesJson = hashedCodesJson;
        
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(hashedCodesJson))
            return false;
        
        try
        {
            var hashedCodes = JsonSerializer.Deserialize<List<string>>(hashedCodesJson);
            if (hashedCodes == null || hashedCodes.Count == 0)
                return false;
            
            var inputHash = HashBackupCode(code);
            var matchIndex = hashedCodes.FindIndex(h => h == inputHash);
            
            if (matchIndex == -1)
                return false;
            
            // Remove the used code
            hashedCodes.RemoveAt(matchIndex);
            updatedHashedCodesJson = JsonSerializer.Serialize(hashedCodes);
            
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    private static string GenerateSingleCode()
    {
        var sb = new StringBuilder(CodeLength);
        
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[CodeLength];
        rng.GetBytes(bytes);
        
        for (int i = 0; i < CodeLength; i++)
        {
            var index = bytes[i] % CodeCharacters.Length;
            sb.Append(CodeCharacters[index]);
            
            // Add dash in the middle for readability (XXXX-XXXX format)
            if (i == 3)
                sb.Append('-');
        }
        
        return sb.ToString();
    }
}
