using Learnify.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Application.Enrollments.Queries.GetEnrollmentById;
public record GetEnrollmentByIdQuery(int Id) : IRequest<Enrollment>;