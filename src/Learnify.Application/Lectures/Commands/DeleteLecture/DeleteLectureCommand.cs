using MediatR;

namespace Learnify.Application.Lectures.Commands.DeleteLecture
{
    public record DeleteLectureCommand(int Id) : IRequest<Unit>;
} 