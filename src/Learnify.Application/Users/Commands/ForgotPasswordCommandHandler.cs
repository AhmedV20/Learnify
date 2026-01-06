using Learnify.Application.Email;
using Learnify.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

namespace Learnify.Application.Users.Commands;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(UserManager<ApplicationUser> userManager, IEmailService emailService)
    {
        _userManager = userManager;
        _emailService = emailService;
    }

    public async Task<ForgotPasswordResult> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            // Don't reveal if user exists (security)
            return new ForgotPasswordResult
            {
                Success = true,
                Message = "If the email exists, a password reset OTP has been sent."
            };
        }

        // Generate OTP
        var otp = GenerateOtp();
        var otpHash = HashOtp(otp);

        // Update user with password reset OTP (invalidates old one)
        user.PasswordResetOtpHash = otpHash;
        user.PasswordResetOtpExpiry = DateTime.UtcNow.AddMinutes(10);
        user.PasswordResetOtpAttempts = 0;
        user.PasswordResetTokenHash = null; // Clear any existing reset token
        user.PasswordResetTokenExpiry = null;
        await _userManager.UpdateAsync(user);

        // Send OTP email
        await _emailService.SendOtpEmailAsync(
            user.Email!,
            user.FirstName ?? user.UserName ?? "User",
            otp,
            "Password Reset");

        return new ForgotPasswordResult
        {
            Success = true,
            Message = "If the email exists, a password reset OTP has been sent."
        };
    }

    private static string GenerateOtp()
    {
        using (var rng = RandomNumberGenerator.Create())
        {
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var number = BitConverter.ToUInt32(bytes, 0) % 1000000;
            return number.ToString("D6");
        }
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
