using Learnify.Application.TwoFactorAuth;
using OtpNet;
using QRCoder;

namespace Learnify.Infrastructure.TwoFactorAuth;

/// <summary>
/// TOTP service implementation using OtpNet and QRCoder
/// </summary>
public class TotpService : ITotpService
{
    private const int SecretKeyLength = 20; // 160 bits as recommended by RFC 4226
    private const int TotpStep = 30; // 30 second time window
    private const int TotpDigits = 6;
    
    /// <inheritdoc />
    public string GenerateSecretKey()
    {
        var key = KeyGeneration.GenerateRandomKey(SecretKeyLength);
        return Base32Encoding.ToString(key);
    }
    
    /// <inheritdoc />
    public string GenerateQrCodeUri(string email, string secretKey, string issuer = "Learnify")
    {
        // Format: otpauth://totp/{issuer}:{email}?secret={secret}&issuer={issuer}&digits=6&period=30
        var encodedIssuer = Uri.EscapeDataString(issuer);
        var encodedEmail = Uri.EscapeDataString(email);
        
        return $"otpauth://totp/{encodedIssuer}:{encodedEmail}?secret={secretKey}&issuer={encodedIssuer}&digits={TotpDigits}&period={TotpStep}";
    }
    
    /// <inheritdoc />
    public string GenerateQrCodeImage(string qrCodeUri)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(qrCodeUri, QRCodeGenerator.ECCLevel.M);
        using var qrCode = new PngByteQRCode(qrCodeData);
        var qrCodeBytes = qrCode.GetGraphic(5); // 5 pixels per module
        
        return Convert.ToBase64String(qrCodeBytes);
    }
    
    /// <inheritdoc />
    public bool ValidateCode(string secretKey, string code)
    {
        if (string.IsNullOrWhiteSpace(secretKey) || string.IsNullOrWhiteSpace(code))
            return false;
            
        if (code.Length != TotpDigits || !code.All(char.IsDigit))
            return false;
        
        try
        {
            var keyBytes = Base32Encoding.ToBytes(secretKey);
            var totp = new Totp(keyBytes, step: TotpStep, totpSize: TotpDigits);
            
            // Allow 1 step window on each side for clock drift
            return totp.VerifyTotp(code, out _, new VerificationWindow(previous: 1, future: 1));
        }
        catch
        {
            return false;
        }
    }
}
