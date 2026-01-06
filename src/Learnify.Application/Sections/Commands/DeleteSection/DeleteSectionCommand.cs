using MediatR;

namespace Learnify.Application.Sections.Commands.DeleteSection;

public record DeleteSectionCommand(int Id) : IRequest; 