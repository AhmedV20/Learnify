using Learnify.Application.Categories.Queries.GetAllCategories;
using Learnify.Application.Common.Interfaces;
using Learnify.Application.Common.Pagination;
using Learnify.Application.Courses.DTOs;
using Learnify.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Application.Enrollments.Queries.GetAllEnrollments;

public class GetAllEnrollmentsQueryHandler : IRequestHandler<GetAllEnrollmentsQuery, (IEnumerable<Enrollment>, int count)>
{
    private readonly IEnrollmentRepository _enrollmentRepository;

    public GetAllEnrollmentsQueryHandler(IEnrollmentRepository enrollmentRepository)
    {
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<(IEnumerable<Enrollment>, int count)> Handle(GetAllEnrollmentsQuery request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(request.InstructorId))
        {
            var (enrollments, totalCount) = await _enrollmentRepository.GetEnrollmentsByInstructorIdAsync(request.InstructorId, request.PageNumber, request.PageSize);
            return (enrollments, totalCount);
        }
        else if (!string.IsNullOrEmpty(request.UserId))
        {
            var (enrollments, totalCount) = await _enrollmentRepository.GetEnrollmentsByUserIdAsync(request.UserId, request.PageNumber, request.PageSize);
            return (enrollments, totalCount);
        }
        else
        {
            var (enrollments, totalCount) = await _enrollmentRepository.GetAllEnrollmentsAsync(request.PageNumber, request.PageSize);
            return (enrollments, totalCount);
        }
    }
}