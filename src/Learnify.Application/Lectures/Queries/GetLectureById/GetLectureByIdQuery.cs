using Learnify.Application.Lectures.DTOs;
using MediatR;

namespace Learnify.Application.Lectures.Queries.GetLectureById
{
    public record GetLectureByIdQuery(int Id) : IRequest<LectureResponse>;
} 