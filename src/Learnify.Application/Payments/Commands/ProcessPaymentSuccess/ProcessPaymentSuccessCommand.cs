using MediatR;

namespace Learnify.Application.Payments.Commands.ProcessPaymentSuccess;

public record ProcessPaymentSuccessCommand(
    string SessionId,
    string UserId
) : IRequest<(bool success, string successUrl)>; 