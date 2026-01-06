using Learnify.Application.Email;
using Learnify.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

namespace Learnify.Application.Users.Commands;

public class ResendEmailOtpCommandHandler : IRequestHandler<ResendEmailOtpCommand, ResendEmailOtpResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;

    public ResendEmailOtpCommandHandler(UserManager<ApplicationUser> userManager, IEmailService emailService)
    {
        _userManager = userManager;
        _emailService = emailService;
    }

    public async Task<ResendEmailOtpResult> Handle(ResendEmailOtpCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            // Don't reveal if user exists
            return new ResendEmailOtpResult
            {
                Success = true,
                Message = "If the email exists, a new OTP has been sent."
            };
        }

        // Check if already confirmed
        if (user.EmailConfirmed)
        {
            return new ResendEmailOtpResult
            {
                Success = false,
                Message = "Email is already verified."
            };
        }

        // Generate new OTP (invalidates old one)
        var otp = GenerateOtp();
        var otpHash = HashOtp(otp);

        // Update user with new OTP
        user.EmailOtpHash = otpHash;
        user.EmailOtpExpiry = DateTime.UtcNow.AddMinutes(10);
        user.EmailOtpAttempts = 0; // Reset attempts
        await _userManager.UpdateAsync(user);

        // Send OTP email
        await _emailService.SendOtpEmailAsync(
            user.Email!,
            user.FirstName ?? user.UserName ?? "User",
            otp,
            "Email Verification");

        return new ResendEmailOtpResult
        {
            Success = true,
            Message = "A new OTP has been sent to your email."
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
