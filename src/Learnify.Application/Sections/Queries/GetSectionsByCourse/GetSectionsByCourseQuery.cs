using Learnify.Application.Sections.DTOs;
using MediatR;
using System.Collections.Generic;

namespace Learnify.Application.Sections.Queries.GetSectionsByCourse;

public record GetSectionsByCourseQuery(int CourseId) : IRequest<IEnumerable<SectionResponse>>; 