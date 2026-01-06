using AutoMapper;
using Learnify.Application.Common.Interfaces;
using Learnify.Application.Lectures.DTOs;
using Learnify.Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Learnify.Application.Lectures.Commands.CreateLecture
{
    public class CreateLectureCommandHandler : IRequestHandler<CreateLectureCommand, LectureResponse>
    {
        private readonly ILectureRepository _lectureRepository;
        private readonly IMapper _mapper;

        public CreateLectureCommandHandler(ILectureRepository lectureRepository, IMapper mapper)
        {
            _lectureRepository = lectureRepository;
            _mapper = mapper;
        }

        public async Task<LectureResponse> Handle(CreateLectureCommand request, CancellationToken cancellationToken)
        {
            var lecture = _mapper.Map<Lecture>(request);

            var newLecture = await _lectureRepository.AddAsync(lecture);

            return _mapper.Map<LectureResponse>(newLecture);
        }
    }
} 