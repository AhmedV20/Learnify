using Learnify.Application.Features.Authentication.Commands;
using MediatR;

namespace Learnify.Application.Users.Commands;

/// <summary>
/// Command to verify 2FA code during login
/// </summary>
public class Verify2FACommand : IRequest<LoginResponse>
{
    /// <summary>
    /// Temporary 2FA token received from login response
    /// </summary>
    public string TwoFactorToken { get; set; } = string.Empty;
    
    /// <summary>
    /// 6-digit verification code (TOTP or Email OTP or Backup Code)
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Set to true if using a backup recovery code instead of TOTP/Email
    /// </summary>
    public bool UseBackupCode { get; set; } = false;
}
