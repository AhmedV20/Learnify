using Learnify.Application.Common.Exceptions;
using Learnify.Application.Common.Interfaces;
using Learnify.Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Learnify.Application.Lectures.Commands.DeleteLecture
{
    public class DeleteLectureCommandHandler : IRequestHandler<DeleteLectureCommand, Unit>
    {
        private readonly ILectureRepository _lectureRepository;

        public DeleteLectureCommandHandler(ILectureRepository lectureRepository)
        {
            _lectureRepository = lectureRepository;
        }

        public async Task<Unit> Handle(DeleteLectureCommand request, CancellationToken cancellationToken)
        {
            var lectureToDelete = await _lectureRepository.GetByIdAsync(request.Id);

            if (lectureToDelete == null)
            {
                throw new NotFoundException(nameof(Lecture), request.Id);
            }

            await _lectureRepository.DeleteAsync(request.Id);

            return Unit.Value;
        }
    }
} 