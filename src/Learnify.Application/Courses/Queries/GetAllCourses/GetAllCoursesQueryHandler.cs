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
    public class GetAllCoursesQueryHandler (ICourseRepository _courseRepository, IMapper _mapper) : IRequestHandler<GetAllCoursesQuery, PagedResult<CourseResponse>>
    {

        public async Task<PagedResult<CourseResponse>> Handle(GetAllCoursesQuery request, CancellationToken cancellationToken)
        {
            var (courses, totalCount) = await _courseRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchQuery);

            var courseResponses = _mapper.Map<List<CourseResponse>>(courses);

           return new PagedResult<CourseResponse>(courseResponses, totalCount, request.PageNumber, request.PageSize);
        }
    }

 }

