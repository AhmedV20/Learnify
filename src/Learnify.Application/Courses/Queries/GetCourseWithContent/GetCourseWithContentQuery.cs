using Learnify.Application.Courses.DTOs;
using MediatR;

namespace Learnify.Application.Courses.Queries.GetCourseWithContent
{
    public record GetCourseWithContentQuery(int CourseId) : IRequest<CourseWithContentResponse>;
} 