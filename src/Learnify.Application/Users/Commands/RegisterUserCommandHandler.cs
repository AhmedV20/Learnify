using Learnify.Application.BackgroundJobs.Interfaces;
using Learnify.Application.Email;
using Learnify.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Learnify.Application.Users.Commands;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IBackgroundJobService _backgroundJobService;

    public RegisterUserCommandHandler(
        UserManager<ApplicationUser> userManager, 
        RoleManager<IdentityRole> roleManager, 
        IBackgroundJobService backgroundJobService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _backgroundJobService = backgroundJobService;
    }

    public async Task<RegisterResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        
        if (existingUser != null)
        {
            // Email exists - check if verified
            if (!existingUser.EmailConfirmed)
            {
                // Email exists but NOT verified - don't auto-send OTP
                // User will click "Verify" button which calls resend-otp endpoint
                return new RegisterResponse
                {
                    Status = AuthResponseStatus.EmailNotVerified,
                    Message = "This email is already registered but not verified. Click 'Verify' to receive a verification code.",
                    UserId = existingUser.Id,
                    Email = existingUser.Email,
                    RequiresVerification = true
                };
            }
            
            // Email exists and IS verified - can't register
            return new RegisterResponse
            {
                Status = AuthResponseStatus.EmailAlreadyExists,
                Message = "An account with this email already exists. Please login instead.",
                Email = request.Email,
                RequiresVerification = false
            };
        }

        // New registration - create user
        var newOtp = GenerateOtp();
        var otpHash = HashOtp(newOtp);

        var user = new ApplicationUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            UserName = request.UserName,
            SecurityStamp = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            EmailOtpHash = otpHash,
            EmailOtpExpiry = DateTime.UtcNow.AddMinutes(10),
            EmailOtpAttempts = 0
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return new RegisterResponse
            {
                Status = AuthResponseStatus.InvalidCredentials,
                Message = $"Registration failed: {errors}",
                RequiresVerification = false
            };
        }

        // Assign default role
        if (await _roleManager.RoleExistsAsync("User"))
        {
            await _userManager.AddToRoleAsync(user, "User");
        }

        // OLD CODE - Keep for 1 week as backup, then remove
        // await _emailService.SendOtpEmailAsync(
        //     user.Email!, 
        //     user.FirstName ?? user.UserName ?? "User", 
        //     newOtp, 
        //     "Email Verification"
        // );

        // NEW CODE - Asynchronous with Hangfire (non-blocking)
        _backgroundJobService.Enqueue<IEmailJobService>(
            service => service.SendOtpEmailAsync(
                user.Email!, 
                user.FirstName ?? user.UserName ?? "User", 
                newOtp, 
                "Email Verification"
            )
        );

        return new RegisterResponse
        {
            Status = AuthResponseStatus.Success,
            Message = "Registration successful! Please check your email for the verification code.",
            UserId = user.Id,
            Email = user.Email,
            RequiresVerification = true
        };
    }

    private static string GenerateOtp()
    {
        // Generate cryptographically secure 6-digit OTP
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        var number = BitConverter.ToUInt32(bytes, 0) % 1000000;
        return number.ToString("D6"); // Ensure 6 digits with leading zeros
    }

    private static string HashOtp(string otp)
    {
        using var sha256 = SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(otp);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}