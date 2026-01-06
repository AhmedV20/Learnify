using AutoMapper;
using Learnify.Application.Common.Interfaces;
using Learnify.Application.Common.Pagination;
using Learnify.Application.Courses.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Application.Courses.Queries.GetAllCourses
{
    public class GetCoursesByCategoryIdQueryHandler : IRequestHandler<GetCoursesByCategoryIdQuery, PagedResult<CourseResponse>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;

        public GetCoursesByCategoryIdQueryHandler(ICourseRepository courseRepository, IMapper mapper)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
        }

        public async Task<PagedResult<CourseResponse>> Handle(GetCoursesByCategoryIdQuery request, CancellationToken cancellationToken)
        {
            var (courses, totalCount) = await _courseRepository.GetByCategoryIdAsync(
                request.CategoryId,
                request.PageNumber,
                request.PageSize);

            var courseResponses = _mapper.Map<List<CourseResponse>>(courses);

            return new PagedResult<CourseResponse>(courseResponses, totalCount, request.PageNumber, request.PageSize);
        }
    }
}
