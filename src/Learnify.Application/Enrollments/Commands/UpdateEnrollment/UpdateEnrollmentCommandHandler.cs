using AutoMapper;
using Learnify.Application.Categories.Commands.UpdateCategory;
using Learnify.Application.Common.Exceptions;
using Learnify.Application.Common.Interfaces;
using Learnify.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Application.Enrollments.Commands.UpdateEnrollment;


public class UpdateEnrollmentCommandHandler : IRequestHandler<UpdateEnrollmentCommand, Enrollment>
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IMapper _mapper;


    public UpdateEnrollmentCommandHandler(IEnrollmentRepository enrollmentRepository, IMapper mapper)
    {
        _enrollmentRepository = enrollmentRepository;
        _mapper=mapper;
    }

    public async Task<Enrollment> Handle(UpdateEnrollmentCommand request, CancellationToken cancellationToken)
    {
        var enrollment = await _enrollmentRepository.GetEnrollmentByIdAsync(request.Id);

        if (enrollment == null)
        {
            throw new NotFoundException(nameof(Enrollment), request.Id);
        }

        enrollment = _mapper.Map<Enrollment>(request);

        await _enrollmentRepository.UpdateEnrollmentAsync(enrollment);

        return enrollment;
    }
}
