using Learnify.Application.Sections.DTOs;
using MediatR;

namespace Learnify.Application.Sections.Queries.GetSectionById;

public record GetSectionByIdQuery(int Id) : IRequest<SectionResponse>; 