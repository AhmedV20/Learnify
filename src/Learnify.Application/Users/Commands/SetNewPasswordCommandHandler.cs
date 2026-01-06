using Learnify.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

namespace Learnify.Application.Users.Commands;

public class SetNewPasswordCommandHandler : IRequestHandler<SetNewPasswordCommand, SetNewPasswordResult>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public SetNewPasswordCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<SetNewPasswordResult> Handle(SetNewPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return new SetNewPasswordResult
            {
                Success = false,
                Message = "Invalid email address."
            };
        }

        // Check if reset token exists
        if (string.IsNullOrEmpty(user.PasswordResetTokenHash))
        {
            return new SetNewPasswordResult
            {
                Success = false,
                Message = "No password reset request found. Please request a new one."
            };
        }

        // Check token expiry
        if (user.PasswordResetTokenExpiry == null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
        {
            // Clear expired token
            user.PasswordResetTokenHash = null;
            user.PasswordResetTokenExpiry = null;
            await _userManager.UpdateAsync(user);

            return new SetNewPasswordResult
            {
                Success = false,
                Message = "Reset token has expired. Please request a new password reset."
            };
        }

        // Verify reset token
        var providedTokenHash = HashValue(request.ResetToken);
        if (providedTokenHash != user.PasswordResetTokenHash)
        {
            return new SetNewPasswordResult
            {
                Success = false,
                Message = "Invalid reset token."
            };
        }

        // Reset the password
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return new SetNewPasswordResult
            {
                Success = false,
                Message = $"Password reset failed: {errors}"
            };
        }

        // Clear reset token after successful password change
        user.PasswordResetTokenHash = null;
        user.PasswordResetTokenExpiry = null;
        await _userManager.UpdateAsync(user);

        return new SetNewPasswordResult
        {
            Success = true,
            Message = "Password has been reset successfully. You can now login with your new password."
        };
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
