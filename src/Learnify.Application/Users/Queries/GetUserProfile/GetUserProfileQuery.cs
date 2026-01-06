using Learnify.Application.Users.DTOs.Response;
using MediatR;

namespace Learnify.Application.Users.Queries.GetUserProfile
{
    public class GetUserProfileQuery : IRequest<UserProfileResponse>
    {
        public string UserId { get; set; }
    }
}