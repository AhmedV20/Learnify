using Learnify.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Learnify.Application.Users.Commands
{
    public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UpdateProfileResult>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UpdateProfileCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<UpdateProfileResult> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return new UpdateProfileResult
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            // Update fields if provided
            if (request.FirstName != null)
            {
                user.FirstName = request.FirstName;
            }
            
            if (request.LastName != null)
            {
                user.LastName = request.LastName;
            }

            user.LastModifiedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return new UpdateProfileResult
                {
                    Success = true,
                    Message = "Profile updated successfully."
                };
            }

            return new UpdateProfileResult
            {
                Success = false,
                Message = string.Join(", ", result.Errors.Select(e => e.Description))
            };
        }
    }
}
