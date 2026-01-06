using Asp.Versioning;
using Learnify.Api.RateLimiting;
using Learnify.Application.Features.Authentication.Commands;
using Learnify.Application.Users.Commands;
using Learnify.Application.Users.Queries.GetUserProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using System.Threading.Tasks;
using YourApp.Application.Features.Authentication.Commands;

namespace Learnify.API.Controllers;

[ApiController]
    [ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[EnableRateLimiting(RateLimitPolicies.General)]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetUserProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var query = new GetUserProfileQuery { UserId = userId };
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    [HttpPost("login")]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        
        // Return appropriate status code based on result
        if (result.Status == Learnify.Application.Users.Commands.AuthResponseStatus.InvalidCredentials)
        {
            return Unauthorized(result);
        }
        
        // 2FA required - return 200 with Requires2FA flag
        if (result.Status == Learnify.Application.Users.Commands.AuthResponseStatus.Requires2FA)
        {
            return Ok(result);
        }
        
        return Ok(result);
    }
    
    /// <summary>
    /// Verify 2FA code during login
    /// </summary>
    [HttpPost("verify-2fa")]
    [EnableRateLimiting(RateLimitPolicies.Otp)]
    public async Task<IActionResult> Verify2FA([FromBody] Verify2FACommand command)
    {
        var result = await _mediator.Send(command);
        
        if (result.Status == Learnify.Application.Users.Commands.AuthResponseStatus.InvalidCredentials ||
            result.Status == Learnify.Application.Users.Commands.AuthResponseStatus.AccountLocked)
        {
            return Unauthorized(result);
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Sign in with Google OAuth
    /// </summary>
    [HttpPost("google-login")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.IdToken))
        {
            return BadRequest(new { Status = "Error", Message = "Google token is required" });
        }

        var command = new Learnify.Application.Users.Commands.GoogleAuth.GoogleLoginCommand 
        { 
            IdToken = request.IdToken 
        };
        var result = await _mediator.Send(command);
        
        return result.Status switch
        {
            Learnify.Application.Users.Commands.AuthResponseStatus.Success => Ok(result),
            Learnify.Application.Users.Commands.AuthResponseStatus.InvalidCredentials => Unauthorized(result),
            Learnify.Application.Users.Commands.AuthResponseStatus.UserBanned => StatusCode(403, result),
            Learnify.Application.Users.Commands.AuthResponseStatus.Error => StatusCode(500, result),
            _ => BadRequest(result)
        };
    }

    /// <summary>
    /// Register a new user. An OTP will be sent to the provided email address.
    /// </summary>
    [HttpPost("register")]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    #region Email Verification OTP

    /// <summary>
    /// Verify email using OTP
    /// </summary>
    [HttpPost("verify-otp")]
    [EnableRateLimiting(RateLimitPolicies.Otp)]
    public async Task<IActionResult> VerifyEmailOtp([FromBody] VerifyEmailOtpCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.Success)
        {
            return Ok(new { 
                Message = result.Message,
                Token = result.Token,
                Expiration = result.Expiration
            });
        }

        return BadRequest(new { Message = result.Message });
    }

    /// <summary>
    /// Resend email verification OTP
    /// </summary>
    [HttpPost("resend-otp")]
    [EnableRateLimiting(RateLimitPolicies.Otp)]
    public async Task<IActionResult> ResendEmailOtp([FromBody] ResendEmailOtpCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.Success)
        {
            return Ok(new { Message = result.Message });
        }

        return BadRequest(new { Message = result.Message });
    }

    #endregion

    #region Password Reset (Forgot Password)

    /// <summary>
    /// Request password reset OTP
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.Success)
        {
            return Ok(new { Message = result.Message });
        }

        return BadRequest(new { Message = result.Message });
    }

    /// <summary>
    /// Verify password reset OTP and get reset token
    /// </summary>
    [HttpPost("verify-reset-otp")]
    public async Task<IActionResult> VerifyPasswordResetOtp([FromBody] VerifyPasswordResetOtpCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.Success)
        {
            return Ok(new { 
                Message = result.Message,
                ResetToken = result.ResetToken
            });
        }

        return BadRequest(new { Message = result.Message });
    }

    /// <summary>
    /// Set new password using reset token
    /// </summary>
    [HttpPost("set-new-password")]
    public async Task<IActionResult> SetNewPassword([FromBody] SetNewPasswordCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.Success)
        {
            return Ok(new { Message = result.Message });
        }

        return BadRequest(new { Message = result.Message });
    }

    #endregion

    #region Change Password (When Logged In)

    /// <summary>
    /// Change password when logged in (requires old password)
    /// </summary>
    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var command = new ChangePasswordCommand
        {
            UserId = userId,
            OldPassword = request.OldPassword,
            NewPassword = request.NewPassword
        };

        var result = await _mediator.Send(command);

        if (result.Success)
        {
            return Ok(new { Message = result.Message });
        }

        return BadRequest(new { Message = result.Message });
    }

    #endregion

    [HttpPost("{userId}/profile-image")]
    public async Task<IActionResult> UpdateProfileImage(string userId, IFormFile profileImage)
    {
        var command = new UpdateProfileImageCommand
        {
            UserId = userId,
            ProfileImage = profileImage
        };

        var imageUrl = await _mediator.Send(command);

        return Ok(new { ProfileImageUrl = imageUrl });
    }

    /// <summary>
    /// Update user profile (first name, last name)
    /// </summary>
    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var command = new UpdateProfileCommand
        {
            UserId = userId,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await _mediator.Send(command);

        if (result.Success)
        {
            return Ok(new { Message = result.Message });
        }

        return BadRequest(new { Message = result.Message });
    }
}

/// <summary>
/// Request model for update profile endpoint
/// </summary>
public class UpdateProfileRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}


/// <summary>
/// Request model for change password endpoint
/// </summary>
public class ChangePasswordRequest
{
    public required string OldPassword { get; set; }
    public required string NewPassword { get; set; }
}

/// <summary>
/// Request model for Google OAuth login
/// </summary>
public class GoogleLoginRequest
{
    public string IdToken { get; set; } = string.Empty;
}