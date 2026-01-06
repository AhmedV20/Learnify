using Asp.Versioning;
using Learnify.Api.RateLimiting;
using Learnify.Application.TwoFactorAuth;
using Learnify.Domain.Entities;
using Learnify.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Hangfire;
using Learnify.Application.Email;
using System.Threading.Tasks;

namespace Learnify.API.Controllers;

/// <summary>
/// Endpoints for managing Two-Factor Authentication settings
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users/2fa")]
[Authorize]
[EnableRateLimiting(RateLimitPolicies.General)]
public class TwoFactorController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITotpService _totpService;
    private readonly IBackupCodeService _backupCodeService;
    private readonly IEmailService _emailService;

    public TwoFactorController(
        UserManager<ApplicationUser> userManager,
        ITotpService totpService,
        IBackupCodeService backupCodeService,
        IEmailService emailService)
    {
        _userManager = userManager;
        _totpService = totpService;
        _backupCodeService = backupCodeService;
        _emailService = emailService;
    }

    /// <summary>
    /// Get current 2FA status
    /// </summary>
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return Unauthorized();

        return Ok(new TwoFactorStatusResponse
        {
            IsEnabled = user.TwoFactorAuthEnabled,
            Method = user.TwoFactorMethod,
            BackupCodesRemaining = user.BackupCodesRemaining
        });
    }

    /// <summary>
    /// Setup Email 2FA
    /// </summary>
    [HttpPost("setup/email")]
    public async Task<IActionResult> SetupEmail()
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return Unauthorized();

        if (user.AuthProvider == "Google")
        {
            return BadRequest(new { Message = "Google accounts are protected by Google's security settings. Please manage 2FA in your Google Account." });
        }

        user.TwoFactorAuthEnabled = true;
        user.TwoFactorMethod = TwoFactorMethod.Email;
        user.TwoFactorSecretKey = null; // Not needed for email
        await _userManager.UpdateAsync(user);
        
        // Send email notification via Hangfire
        BackgroundJob.Enqueue(() => _emailService.SendTwoFactorEnabledEmailAsync(
            user.Email!, user.FirstName ?? "User", "Email"));

        return Ok(new { Message = "Email 2FA enabled successfully" });
    }

    /// <summary>
    /// Get QR code and secret for Authenticator app setup
    /// </summary>
    [HttpGet("setup/authenticator")]
    public async Task<IActionResult> GetAuthenticatorSetup()
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return Unauthorized();

        if (user.AuthProvider == "Google")
        {
            return BadRequest(new { Message = "Google accounts are protected by Google's security settings. Please manage 2FA in your Google Account." });
        }

        // Generate or retrieve secret key
        var secretKey = _totpService.GenerateSecretKey();
        var qrCodeUri = _totpService.GenerateQrCodeUri(user.Email!, secretKey);
        var qrCodeImage = _totpService.GenerateQrCodeImage(qrCodeUri);

        // Store temporarily (not enabled yet until verified)
        user.TwoFactorSecretKey = secretKey;
        await _userManager.UpdateAsync(user);

        return Ok(new AuthenticatorSetupResponse
        {
            SecretKey = secretKey,
            QrCodeUri = qrCodeUri,
            QrCodeImage = $"data:image/png;base64,{qrCodeImage}"
        });
    }

    /// <summary>
    /// Verify and enable Authenticator 2FA
    /// </summary>
    [HttpPost("setup/authenticator/verify")]
    public async Task<IActionResult> VerifyAuthenticatorSetup([FromBody] VerifyCodeRequest request)
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return Unauthorized();

        if (string.IsNullOrEmpty(user.TwoFactorSecretKey))
        {
            return BadRequest(new { Message = "Please start authenticator setup first" });
        }

        if (!_totpService.ValidateCode(user.TwoFactorSecretKey, request.Code))
        {
            return BadRequest(new { Message = "Invalid verification code" });
        }

        user.TwoFactorAuthEnabled = true;
        user.TwoFactorMethod = TwoFactorMethod.Authenticator;
        await _userManager.UpdateAsync(user);
        
        // Send email notification via Hangfire
        BackgroundJob.Enqueue(() => _emailService.SendTwoFactorEnabledEmailAsync(
            user.Email!, user.FirstName ?? "User", "Authenticator"));

        return Ok(new { Message = "Authenticator 2FA enabled successfully" });
    }

    /// <summary>
    /// Disable 2FA (requires password)
    /// </summary>
    [HttpPost("disable")]
    public async Task<IActionResult> Disable([FromBody] DisableTwoFactorRequest request)
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return Unauthorized();

        // Verification logic
        if (user.AuthProvider == "Google")
        {
            // Google users must type "DISABLE" to confirm
            if (request.Password != "DISABLE")
            {
                return BadRequest(new { Message = "Please type 'DISABLE' to confirm." });
            }
        }
        else
        {
            // Normal users must provide valid password
            if (string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { Message = "Password is required" });
            }

            if (!await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return BadRequest(new { Message = "Invalid password" });
            }
        }

        user.TwoFactorAuthEnabled = false;
        user.TwoFactorMethod = null;
        user.TwoFactorSecretKey = null;
        user.BackupCodesHash = null;
        user.BackupCodesRemaining = 0;
        await _userManager.UpdateAsync(user);
        
        // Send email notification via Hangfire
        BackgroundJob.Enqueue(() => _emailService.SendTwoFactorDisabledEmailAsync(
            user.Email!, user.FirstName ?? "User"));

        return Ok(new { Message = "2FA disabled successfully" });
    }

    /// <summary>
    /// Generate backup recovery codes
    /// </summary>
    [HttpPost("backup-codes")]
    public async Task<IActionResult> GenerateBackupCodes()
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return Unauthorized();

        if (user.AuthProvider == "Google")
        {
            return BadRequest(new { Message = "Google accounts use Google's account recovery settings." });
        }

        if (!user.TwoFactorAuthEnabled)
        {
            return BadRequest(new { Message = "2FA must be enabled first" });
        }

        var codes = _backupCodeService.GenerateBackupCodes(10);
        var hashedCodes = codes.Select(c => _backupCodeService.HashBackupCode(c)).ToList();
        
        user.BackupCodesHash = JsonSerializer.Serialize(hashedCodes);
        user.BackupCodesRemaining = codes.Count;
        await _userManager.UpdateAsync(user);

        return Ok(new BackupCodesResponse
        {
            Codes = codes,
            Message = "Save these codes securely. They can only be shown once!"
        });
    }

    /// <summary>
    /// Regenerate backup codes (invalidates old ones)
    /// </summary>
    [HttpPost("backup-codes/regenerate")]
    public async Task<IActionResult> RegenerateBackupCodes([FromBody] RegenerateBackupCodesRequest request)
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return Unauthorized();

        if (user.AuthProvider == "Google")
        {
            return BadRequest(new { Message = "Google accounts use Google's account recovery settings." });
        }

        // Verify password
            if (!await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return BadRequest(new { Message = "Invalid password" });
            }

        if (!user.TwoFactorAuthEnabled)
        {
            return BadRequest(new { Message = "2FA must be enabled first" });
        }

        var codes = _backupCodeService.GenerateBackupCodes(10);
        var hashedCodes = codes.Select(c => _backupCodeService.HashBackupCode(c)).ToList();
        
        user.BackupCodesHash = JsonSerializer.Serialize(hashedCodes);
        user.BackupCodesRemaining = codes.Count;
        await _userManager.UpdateAsync(user);

        return Ok(new BackupCodesResponse
        {
            Codes = codes,
            Message = "New backup codes generated. Old codes are now invalid!"
        });
    }

    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return null;
        return await _userManager.FindByIdAsync(userId);
    }
}

#region DTOs

public class TwoFactorStatusResponse
{
    public bool IsEnabled { get; set; }
    public TwoFactorMethod? Method { get; set; }
    public int BackupCodesRemaining { get; set; }
}

public class AuthenticatorSetupResponse
{
    public string SecretKey { get; set; } = string.Empty;
    public string QrCodeUri { get; set; } = string.Empty;
    public string QrCodeImage { get; set; } = string.Empty;
}

public class VerifyCodeRequest
{
    public string Code { get; set; } = string.Empty;
}

public class DisableTwoFactorRequest
{
    public string Password { get; set; } = string.Empty;
}

public class RegenerateBackupCodesRequest
{
    public string Password { get; set; } = string.Empty;
}

public class BackupCodesResponse
{
    public List<string> Codes { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}

#endregion
