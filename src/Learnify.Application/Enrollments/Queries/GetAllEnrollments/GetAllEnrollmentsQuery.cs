using Learnify.Application.Common.Pagination;
using Learnify.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Application.Enrollments.Queries.GetAllEnrollments;
public record GetAllEnrollmentsQuery(int PageNumber = 1, int PageSize = 10, string? UserId = null, string? InstructorId = null) : IRequest<(IEnumerable<Enrollment>, int count)>;
