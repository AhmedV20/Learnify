using Learnify.Application.BackgroundJobs.Interfaces;
using Learnify.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Learnify.Application.Users.Commands;

public class VerifyEmailOtpCommandHandler : IRequestHandler<VerifyEmailOtpCommand, VerifyEmailOtpResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IBackgroundJobService _backgroundJobService;
    private const int MaxAttempts = 5;

    public VerifyEmailOtpCommandHandler(
        UserManager<ApplicationUser> userManager, 
        IConfiguration configuration,
        IBackgroundJobService backgroundJobService)
    {
        _userManager = userManager;
        _configuration = configuration;
        _backgroundJobService = backgroundJobService;
    }

    public async Task<VerifyEmailOtpResult> Handle(VerifyEmailOtpCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return new VerifyEmailOtpResult
            {
                Success = false,
                Message = "Invalid email address."
            };
        }

        // Check if already confirmed
        if (user.EmailConfirmed)
        {
            return new VerifyEmailOtpResult
            {
                Success = true,
                Message = "Email is already verified."
            };
        }

        // Check if OTP exists
        if (string.IsNullOrEmpty(user.EmailOtpHash))
        {
            return new VerifyEmailOtpResult
            {
                Success = false,
                Message = "No OTP found. Please request a new one."
            };
        }

        // Check max attempts
        if (user.EmailOtpAttempts >= MaxAttempts)
        {
            // Clear OTP after max attempts
            user.EmailOtpHash = null;
            user.EmailOtpExpiry = null;
            user.EmailOtpAttempts = 0;
            await _userManager.UpdateAsync(user);

            return new VerifyEmailOtpResult
            {
                Success = false,
                Message = "Maximum attempts exceeded. Please request a new OTP."
            };
        }

        // Check expiry
        if (user.EmailOtpExpiry == null || user.EmailOtpExpiry < DateTime.UtcNow)
        {
            // Clear expired OTP
            user.EmailOtpHash = null;
            user.EmailOtpExpiry = null;
            user.EmailOtpAttempts = 0;
            await _userManager.UpdateAsync(user);

            return new VerifyEmailOtpResult
            {
                Success = false,
                Message = "OTP has expired. Please request a new one."
            };
        }

        // Hash the provided OTP and compare
        var providedOtpHash = HashOtp(request.Otp);
        if (providedOtpHash != user.EmailOtpHash)
        {
            // Increment attempts
            user.EmailOtpAttempts++;
            await _userManager.UpdateAsync(user);

            var remainingAttempts = MaxAttempts - user.EmailOtpAttempts;
            return new VerifyEmailOtpResult
            {
                Success = false,
                Message = $"Invalid OTP. {remainingAttempts} attempts remaining."
            };
        }

        // OTP is valid - confirm email and clear OTP fields
        user.EmailConfirmed = true;
        user.EmailOtpHash = null;
        user.EmailOtpExpiry = null;
        user.EmailOtpAttempts = 0;
        await _userManager.UpdateAsync(user);

        // Send welcome email via Hangfire (non-blocking)
        _backgroundJobService.Enqueue<IEmailJobService>(
            service => service.SendWelcomeEmailAsync(
                user.Email!,
                user.FirstName ?? user.UserName ?? "User"
            )
        );

        // Generate JWT token for auto-login
        var (token, expiration) = await GenerateJwtToken(user);

        return new VerifyEmailOtpResult
        {
            Success = true,
            Message = "Email verified successfully!",
            Token = token,
            Expiration = expiration
        };
    }

    private async Task<(string token, DateTime expiration)> GenerateJwtToken(ApplicationUser user)
    {
        // Clean JWT claims - only essential information
        var authClaims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName ?? ""),
            new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, 
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), 
                ClaimValueTypes.Integer64),
        };

        var userRoles = await _userManager.GetRolesAsync(user);
        foreach (var userRole in userRoles)
        {
            authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            authClaims.Add(new Claim("role", userRole));
        }

        var authSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["JWT:Secret"] ?? throw new InvalidOperationException("JWT:Secret not configured"))
        );
        
        // Use configurable expiry from appsettings (default 24 hours)
        var expiryHours = _configuration.GetValue<int>("JWT:TokenExpiryHours", 24);
        var expiration = DateTime.UtcNow.AddHours(expiryHours);

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:ValidIssuer"],
            audience: _configuration["JWT:ValidAudience"],
            expires: expiration,
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        return (tokenHandler.WriteToken(token), expiration);
    }

    private static string HashOtp(string otp)
    {
        using (var sha256 = SHA256.Create())
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(otp);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}
