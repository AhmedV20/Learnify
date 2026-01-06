using MediatR;

namespace Learnify.Application.Users.Commands
{
    public class UpdateProfileCommand : IRequest<UpdateProfileResult>
    {
        public string UserId { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }

    public class UpdateProfileResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
