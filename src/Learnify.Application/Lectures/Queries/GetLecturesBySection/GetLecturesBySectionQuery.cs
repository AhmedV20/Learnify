using Learnify.Application.Lectures.DTOs;
using MediatR;
using System.Collections.Generic;

namespace Learnify.Application.Lectures.Queries.GetLecturesBySection
{
    public record GetLecturesBySectionQuery(int SectionId) : IRequest<IEnumerable<LectureResponse>>;
} 