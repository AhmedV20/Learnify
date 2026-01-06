using Learnify.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

namespace Learnify.Application.Users.Commands;

public class VerifyPasswordResetOtpCommandHandler : IRequestHandler<VerifyPasswordResetOtpCommand, VerifyPasswordResetOtpResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private const int MaxAttempts = 5;

    public VerifyPasswordResetOtpCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<VerifyPasswordResetOtpResult> Handle(VerifyPasswordResetOtpCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return new VerifyPasswordResetOtpResult
            {
                Success = false,
                Message = "Invalid email address."
            };
        }

        // Check if OTP exists
        if (string.IsNullOrEmpty(user.PasswordResetOtpHash))
        {
            return new VerifyPasswordResetOtpResult
            {
                Success = false,
                Message = "No password reset request found. Please request a new one."
            };
        }

        // Check max attempts
        if (user.PasswordResetOtpAttempts >= MaxAttempts)
        {
            // Clear OTP after max attempts
            user.PasswordResetOtpHash = null;
            user.PasswordResetOtpExpiry = null;
            user.PasswordResetOtpAttempts = 0;
            await _userManager.UpdateAsync(user);

            return new VerifyPasswordResetOtpResult
            {
                Success = false,
                Message = "Maximum attempts exceeded. Please request a new OTP."
            };
        }

        // Check expiry
        if (user.PasswordResetOtpExpiry == null || user.PasswordResetOtpExpiry < DateTime.UtcNow)
        {
            // Clear expired OTP
            user.PasswordResetOtpHash = null;
            user.PasswordResetOtpExpiry = null;
            user.PasswordResetOtpAttempts = 0;
            await _userManager.UpdateAsync(user);

            return new VerifyPasswordResetOtpResult
            {
                Success = false,
                Message = "OTP has expired. Please request a new one."
            };
        }

        // Hash the provided OTP and compare
        var providedOtpHash = HashValue(request.Otp);
        if (providedOtpHash != user.PasswordResetOtpHash)
        {
            // Increment attempts
            user.PasswordResetOtpAttempts++;
            await _userManager.UpdateAsync(user);

            var remainingAttempts = MaxAttempts - user.PasswordResetOtpAttempts;
            return new VerifyPasswordResetOtpResult
            {
                Success = false,
                Message = $"Invalid OTP. {remainingAttempts} attempts remaining."
            };
        }

        // OTP is valid - generate reset token (valid for 10 minutes)
        var resetToken = GenerateSecureToken();
        var resetTokenHash = HashValue(resetToken);

        // Clear OTP and set reset token
        user.PasswordResetOtpHash = null;
        user.PasswordResetOtpExpiry = null;
        user.PasswordResetOtpAttempts = 0;
        user.PasswordResetTokenHash = resetTokenHash;
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddMinutes(10);
        await _userManager.UpdateAsync(user);

        return new VerifyPasswordResetOtpResult
        {
            Success = true,
            Message = "OTP verified successfully. Use the reset token to set your new password.",
            ResetToken = resetToken
        };
    }

    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        return Convert.ToHexString(randomBytes).ToLowerInvariant();
    }

    private static string HashValue(string value)
    {
        using (var sha256 = SHA256.Create())
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(value);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}
