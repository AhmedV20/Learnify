using AutoMapper;
using Learnify.Application.Common.Exceptions;
using Learnify.Application.Common.Interfaces;
using Learnify.Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Learnify.Application.Lectures.Commands.UpdateLecture
{
    public class UpdateLectureCommandHandler : IRequestHandler<UpdateLectureCommand, Unit>
    {
        private readonly ILectureRepository _lectureRepository;
        private readonly IMapper _mapper;

        public UpdateLectureCommandHandler(ILectureRepository lectureRepository, IMapper mapper)
        {
            _lectureRepository = lectureRepository;
            _mapper = mapper;
        }

        public async Task<Unit> Handle(UpdateLectureCommand request, CancellationToken cancellationToken)
        {
            var lectureToUpdate = await _lectureRepository.GetByIdAsync(request.Id);

            if (lectureToUpdate == null)
            {
                throw new NotFoundException(nameof(Lecture), request.Id);
            }

            _mapper.Map(request, lectureToUpdate);

            await _lectureRepository.UpdateAsync(lectureToUpdate);

            return Unit.Value;
        }
    }
} 