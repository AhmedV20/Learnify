using Google.Apis.Auth;
using Learnify.Application.Features.Authentication.Commands;
using Learnify.Application.Users.Commands;
using Learnify.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Learnify.Application.Users.Commands.GoogleAuth;

/// <summary>
/// Handler for Google OAuth sign-in with comprehensive error handling
/// </summary>
public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, LoginResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _config;
    private readonly ILogger<GoogleLoginCommandHandler> _logger;

    public GoogleLoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        IConfiguration config,
        ILogger<GoogleLoginCommandHandler> logger)
    {
        _userManager = userManager;
        _config = config;
        _logger = logger;
    }

    public async Task<LoginResponse> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate request
            if (string.IsNullOrWhiteSpace(request.IdToken))
            {
                return ErrorResponse(AuthResponseStatus.InvalidCredentials, "Google token is required");
            }

            // 1. Validate Google ID token
            GoogleJsonWebSignature.Payload payload;
            try
            {
                var clientId = _config["Google:ClientId"];
                if (string.IsNullOrEmpty(clientId))
                {
                    _logger.LogError("Google ClientId not configured in appsettings");
                    return ErrorResponse(AuthResponseStatus.Error, "Google authentication is not configured");
                }

                payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { clientId }
                    });
            }
            catch (InvalidJwtException ex)
            {
                _logger.LogWarning(ex, "Invalid Google token received");
                return ErrorResponse(AuthResponseStatus.InvalidCredentials, "Invalid or expired Google token");
            }

            // 2. Check if Google email is verified
            if (!payload.EmailVerified)
            {
                return ErrorResponse(AuthResponseStatus.RequiresVerification, "Google email is not verified");
            }

            // 3. Find existing user by GoogleId OR Email
            var user = await FindOrCreateUserAsync(payload);
            if (user == null)
            {
                return ErrorResponse(AuthResponseStatus.Error, "Failed to create or find user account");
            }

            // 4. Check if user is active
            if (!user.IsActive)
            {
                return ErrorResponse(AuthResponseStatus.UserBanned, "Your account has been suspended");
            }

            // 5. Generate JWT token
            var token = await GenerateJwtTokenAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            _logger.LogInformation("User {Email} logged in via Google", user.Email);

            return new LoginResponse
            {
                Status = AuthResponseStatus.Success,
                Message = "Login successful",
                Token = token.Token,
                Expiration = token.Expiration,
                UserData = new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    SecondName = user.LastName,
                    Email = user.Email,
                    ProfileImageUrl = user.ProfileImageUrl,
                    Roles = roles.ToList()
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Google login");
            return ErrorResponse(AuthResponseStatus.Error, "An unexpected error occurred. Please try again.");
        }
    }

    private async Task<ApplicationUser?> FindOrCreateUserAsync(GoogleJsonWebSignature.Payload payload)
    {
        // First, try to find by GoogleId (fastest)
        var userByGoogleId = await _userManager.FindByLoginAsync("Google", payload.Subject);
        
        // If not found by GoogleId, check by email
        var user = await _userManager.FindByEmailAsync(payload.Email);

        if (user == null)
        {
            // NEW USER - Create account
            user = new ApplicationUser
            {
                UserName = payload.Email,
                Email = payload.Email,
                EmailConfirmed = true, // Google emails are pre-verified
                FirstName = payload.GivenName,
                LastName = payload.FamilyName,
                ProfileImageUrl = payload.Picture,
                GoogleId = payload.Subject,
                AuthProvider = "Google",
                IsCustomProfilePic = false, // Using Google profile pic initially
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                _logger.LogError("Failed to create user: {Errors}", 
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
                return null;
            }

            // Add default role
            await _userManager.AddToRoleAsync(user, "User");
            
            // Add Google login provider
            await _userManager.AddLoginAsync(user, new UserLoginInfo("Google", payload.Subject, "Google"));
            
            _logger.LogInformation("Created new user via Google: {Email}", user.Email);
        }
        else
        {
            // EXISTING USER - Link or update
            bool needsUpdate = false;

            // Link Google account if not already linked
            if (string.IsNullOrEmpty(user.GoogleId))
            {
                user.GoogleId = payload.Subject;
                user.AuthProvider = user.AuthProvider ?? "Google";
                await _userManager.AddLoginAsync(user, new UserLoginInfo("Google", payload.Subject, "Google"));
                needsUpdate = true;
                _logger.LogInformation("Linked Google account to existing user: {Email}", user.Email);
            }
            else if (user.GoogleId != payload.Subject)
            {
                // Different Google account trying to use same email - security concern
                _logger.LogWarning("Google ID mismatch for email {Email}. Stored: {Stored}, Received: {Received}", 
                    user.Email, user.GoogleId, payload.Subject);
                // Allow login but don't update GoogleId (user may have changed Google account)
            }

            // Update profile picture ONLY if user hasn't uploaded custom pic
            if (!user.IsCustomProfilePic && !string.IsNullOrEmpty(payload.Picture))
            {
                user.ProfileImageUrl = payload.Picture;
                needsUpdate = true;
            }

            // Mark email as confirmed if not already
            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                await _userManager.UpdateAsync(user);
            }
        }

        return user;
    }

    private async Task<TokenResponse> GenerateJwtTokenAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Add role claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiryHours = int.Parse(_config["JWT:TokenExpiryHours"] ?? "168");
        var expiration = DateTime.UtcNow.AddHours(expiryHours);

        var token = new JwtSecurityToken(
            issuer: _config["JWT:ValidIssuer"],
            audience: _config["JWT:ValidAudience"],
            claims: claims,
            expires: expiration,
            signingCredentials: creds
        );

        return new TokenResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiration = expiration
        };
    }

    private static LoginResponse ErrorResponse(AuthResponseStatus status, string message)
    {
        return new LoginResponse
        {
            Status = status,
            Message = message
        };
    }
}
