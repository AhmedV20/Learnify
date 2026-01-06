using MediatR;

namespace Learnify.Application.Carts.Commands
{
    public record DeleteCartCommand(string UserId) : IRequest<bool>;
} 