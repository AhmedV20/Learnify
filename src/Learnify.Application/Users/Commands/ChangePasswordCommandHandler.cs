using Learnify.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Learnify.Application.Users.Commands;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, ChangePasswordResult>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ChangePasswordCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ChangePasswordResult> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return new ChangePasswordResult
            {
                Success = false,
                Message = "User not found."
            };
        }

        // Verify old password
        var isOldPasswordValid = await _userManager.CheckPasswordAsync(user, request.OldPassword);
        if (!isOldPasswordValid)
        {
            return new ChangePasswordResult
            {
                Success = false,
                Message = "Current password is incorrect."
            };
        }

        // Change password
        var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return new ChangePasswordResult
            {
                Success = false,
                Message = $"Password change failed: {errors}"
            };
        }

        return new ChangePasswordResult
        {
            Success = true,
            Message = "Password changed successfully."
        };
    }
}
