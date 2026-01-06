using MediatR;

namespace Learnify.Application.Enrollments.Queries.CheckEnrollment;

public record CheckEnrollmentQuery(string UserId, int CourseId) : IRequest<CheckEnrollmentResponse>; 