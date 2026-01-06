using Learnify.Application.Common.Exceptions;
using Learnify.Application.Common.Interfaces;
using Learnify.Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Learnify.Application.Lectures.Commands.ToggleLecturePreview
{
    public class ToggleLecturePreviewCommandHandler : IRequestHandler<ToggleLecturePreviewCommand, Unit>
    {
        private readonly ILectureRepository _lectureRepository;

        public ToggleLecturePreviewCommandHandler(ILectureRepository lectureRepository)
        {
            _lectureRepository = lectureRepository;
        }

        public async Task<Unit> Handle(ToggleLecturePreviewCommand request, CancellationToken cancellationToken)
        {
            var lecture = await _lectureRepository.GetByIdAsync(request.Id);
            if (lecture == null)
            {
                throw new NotFoundException(nameof(Lecture), request.Id);
            }

            lecture.IsPreviewAllowed = request.IsPreviewAllowed;
            await _lectureRepository.UpdateAsync(lecture);

            return Unit.Value;
        }
    }
} 