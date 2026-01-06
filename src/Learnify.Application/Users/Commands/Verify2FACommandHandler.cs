using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Learnify.Application.Common.Interfaces;
using Learnify.Application.Features.Authentication.Commands;
using Learnify.Application.TwoFactorAuth;
using Learnify.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Learnify.Application.Users.Commands;

public class Verify2FACommandHandler : IRequestHandler<Verify2FACommand, LoginResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IImageUrlService _imageUrlService;
    private readonly ITotpService _totpService;
    private readonly IBackupCodeService _backupCodeService;
    
    private const int MaxAttempts = 5;

    public Verify2FACommandHandler(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        IImageUrlService imageUrlService,
        ITotpService totpService,
        IBackupCodeService backupCodeService)
    {
        _userManager = userManager;
        _configuration = configuration;
        _imageUrlService = imageUrlService;
        _totpService = totpService;
        _backupCodeService = backupCodeService;
    }

    public async Task<LoginResponse> Handle(Verify2FACommand request, CancellationToken cancellationToken)
    {
        // 1. Find user by 2FA token
        var tokenHash = HashValue(request.TwoFactorToken);
        var users = _userManager.Users
            .Where(u => u.TwoFactorTokenHash == tokenHash)
            .ToList();
        
        var user = users.FirstOrDefault();

        if (user == null)
        {
            return new LoginResponse
            {
                Status = AuthResponseStatus.InvalidCredentials,
                Message = "Invalid or expired 2FA session. Please login again."
            };
        }

        // 2. Check if token is expired
        if (user.TwoFactorTokenExpiry == null || user.TwoFactorTokenExpiry < DateTime.UtcNow)
        {
            // Clear expired token
            user.TwoFactorTokenHash = null;
            user.TwoFactorTokenExpiry = null;
            await _userManager.UpdateAsync(user);
            
            return new LoginResponse
            {
                Status = AuthResponseStatus.InvalidCredentials,
                Message = "2FA session expired. Please login again."
            };
        }

        // 3. Validate the code based on method
        bool isValid = false;

        if (request.UseBackupCode)
        {
            // Validate backup code
            if (!string.IsNullOrEmpty(user.BackupCodesHash))
            {
                isValid = _backupCodeService.ValidateAndConsumeBackupCode(
                    request.Code, 
                    user.BackupCodesHash, 
                    out var updatedCodes);
                
                if (isValid)
                {
                    user.BackupCodesHash = updatedCodes;
                    user.BackupCodesRemaining--;
                }
            }
        }
        else if (user.TwoFactorMethod == Learnify.Domain.Enums.TwoFactorMethod.Authenticator)
        {
            // Validate TOTP code
            if (!string.IsNullOrEmpty(user.TwoFactorSecretKey))
            {
                isValid = _totpService.ValidateCode(user.TwoFactorSecretKey, request.Code);
            }
        }
        else // Email method
        {
            // Check attempts
            if (user.TwoFactorOtpAttempts >= MaxAttempts)
            {
                return new LoginResponse
                {
                    Status = AuthResponseStatus.AccountLocked,
                    Message = "Too many failed attempts. Please login again to receive a new code."
                };
            }

            // Validate Email OTP
            if (user.TwoFactorOtpExpiry != null && user.TwoFactorOtpExpiry > DateTime.UtcNow)
            {
                var codeHash = HashValue(request.Code);
                isValid = user.TwoFactorOtpHash == codeHash;
            }

            if (!isValid)
            {
                user.TwoFactorOtpAttempts++;
                await _userManager.UpdateAsync(user);
            }
        }

        if (!isValid)
        {
            return new LoginResponse
            {
                Status = AuthResponseStatus.InvalidCredentials,
                Message = "Invalid verification code. Please try again."
            };
        }

        // 4. Clear 2FA session data
        user.TwoFactorTokenHash = null;
        user.TwoFactorTokenExpiry = null;
        user.TwoFactorOtpHash = null;
        user.TwoFactorOtpExpiry = null;
        user.TwoFactorOtpAttempts = 0;
        await _userManager.UpdateAsync(user);

        // 5. Generate JWT token (same as LoginCommandHandler)
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

        var token = CreateToken(authClaims);
        var tokenHandler = new JwtSecurityTokenHandler();

        var userDataResponse = new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            SecondName = user.LastName,
            Email = user.Email,
            ProfileImageUrl = _imageUrlService.GetFullUrl(user.ProfileImageUrl),
            Roles = userRoles.ToList(),
        };

        return new LoginResponse
        {
            Status = AuthResponseStatus.Success,
            Message = "Login successful",
            Token = tokenHandler.WriteToken(token),
            Expiration = token.ValidTo,
            UserData = userDataResponse,
            RequiresVerification = false
        };
    }

    private JwtSecurityToken CreateToken(List<Claim> authClaims)
    {
        var authSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["JWT:Secret"] ?? throw new InvalidOperationException("JWT:Secret not configured"))
        );

        var expiryHours = _configuration.GetValue<int>("JWT:TokenExpiryHours", 24);

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:ValidIssuer"],
            audience: _configuration["JWT:ValidAudience"],
            expires: DateTime.UtcNow.AddHours(expiryHours),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return token;
    }

    private static string HashValue(string value)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(value);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
