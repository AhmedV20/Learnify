using Learnify.Application.Sections.DTOs;
using MediatR;

namespace Learnify.Application.Sections.Commands.CreateSection;

public class CreateSectionCommand : IRequest<SectionResponse>
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Order { get; set; }
} 