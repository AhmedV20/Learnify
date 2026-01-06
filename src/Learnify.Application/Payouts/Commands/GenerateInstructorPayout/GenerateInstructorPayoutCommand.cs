using Learnify.Domain.Entities;
using MediatR;

namespace Learnify.Application.Payouts.Commands.GenerateInstructorPayout;

public class GenerateInstructorPayoutCommand : IRequest<InstructorPayout>
{
    public string InstructorId { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
    public string? Notes { get; set; }
} 