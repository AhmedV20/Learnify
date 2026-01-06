using AutoMapper;
using Learnify.Application.Common.Exceptions;
using Learnify.Application.Common.Interfaces;
using Learnify.Application.Courses.DTOs;
using Learnify.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Application.Courses.Queries.GetCourseById
{
    public class GetCourseByIdQueryHandler (ICourseRepository _courseRepository , IMapper _mapper) : IRequestHandler<GetCourseByIdQuery, CourseResponse>
    {
        public async Task<CourseResponse> Handle(GetCourseByIdQuery request, CancellationToken cancellationToken)
        {

            var course = await _courseRepository.GetByIdAsync(request.Id);


            // Mafrod n handle el errors hna 

            if(course == null)
            {
                throw new  NotFoundException(nameof(Course),request.Id);
            }

            return _mapper.Map<CourseResponse>(course);



        }
    }
}
