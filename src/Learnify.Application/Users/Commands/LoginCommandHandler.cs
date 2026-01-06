using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Learnify.Application.Common.Interfaces;
using Learnify.Application.Email;
using Learnify.Application.Features.Authentication.Commands;
using Learnify.Application.Users.Commands;
using Learnify.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace YourApp.Application.Features.Authentication.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IImageUrlService _imageUrlService;
    private readonly IEmailService _emailService;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager, 
        IConfiguration configuration, 
        IImageUrlService imageUrlService,
        IEmailService emailService)
    {
        _userManager = userManager;
        _configuration = configuration;
        _imageUrlService = imageUrlService;
        _emailService = emailService;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 1. Find user by email
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            return new LoginResponse
            {
                Status = AuthResponseStatus.InvalidCredentials,
                Message = "Invalid email or password."
            };
        }

        // 2. Check password
        if (!await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return new LoginResponse
            {
                Status = AuthResponseStatus.InvalidCredentials,
                Message = "Invalid email or password."
            };
        }

        // 3. Check if email is confirmed
        if (!user.EmailConfirmed)
        {
            // Don't auto-send OTP here - user will click "Verify" button which calls resend-otp
            return new LoginResponse
            {
                Status = AuthResponseStatus.EmailNotVerified,
                Message = "Your email is not verified. Please verify your email to continue.",
                Email = user.Email,
                RequiresVerification = true
            };
        }

        // 4. Check if user is active
        if (!user.IsActive)
        {
            return new LoginResponse
            {
                Status = AuthResponseStatus.AccountDeactivated,
                Message = "Your account has been deactivated. Please contact support."
            };
        }

        // 5. Check if 2FA is enabled
        if (user.TwoFactorAuthEnabled)
        {
            // Generate a temporary 2FA session token (valid for 5 minutes)
            var twoFactorToken = Guid.NewGuid().ToString("N");
            user.TwoFactorTokenHash = HashOtp(twoFactorToken);
            user.TwoFactorTokenExpiry = DateTime.UtcNow.AddMinutes(5);
            
            // If Email method, send OTP
            if (user.TwoFactorMethod == Learnify.Domain.Enums.TwoFactorMethod.Email)
            {
                var otp = GenerateOtp();
                user.TwoFactorOtpHash = HashOtp(otp);
                user.TwoFactorOtpExpiry = DateTime.UtcNow.AddMinutes(5);
                user.TwoFactorOtpAttempts = 0;
                
                await _userManager.UpdateAsync(user);
                await _emailService.SendOtpEmailAsync(user.Email!, user.FirstName ?? "User", otp, "Two-Factor Authentication");
            }
            else
            {
                await _userManager.UpdateAsync(user);
            }
            
            return new LoginResponse
            {
                Status = AuthResponseStatus.Requires2FA,
                Message = "Two-factor authentication required",
                Requires2FA = true,
                TwoFactorToken = twoFactorToken,
                TwoFactorMethod = user.TwoFactorMethod,
                Email = MaskEmail(user.Email)
            };
        }

        // 6. Build the claims - clean, essential claims only
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

        // 6. Generate the token
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

        // 7. Return the response
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

    private static string GenerateOtp()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        var number = BitConverter.ToUInt32(bytes, 0) % 1000000;
        return number.ToString("D6");
    }

    private static string HashOtp(string otp)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(otp);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
    
    private static string? MaskEmail(string? email)
    {
        if (string.IsNullOrEmpty(email)) return null;
        
        var atIndex = email.IndexOf('@');
        if (atIndex <= 1) return email;
        
        var localPart = email[..atIndex];
        var domain = email[atIndex..];
        
        // Show first 2 chars, mask the rest
        var visible = Math.Min(2, localPart.Length);
        var masked = localPart[..visible] + new string('*', Math.Max(0, localPart.Length - visible));
        
        return masked + domain;
    }
}