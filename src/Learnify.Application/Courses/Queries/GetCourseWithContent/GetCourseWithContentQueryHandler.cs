using AutoMapper;
using Learnify.Application.Common.Exceptions;
using Learnify.Application.Common.Interfaces;
using Learnify.Application.Courses.DTOs;
using Learnify.Application.Lectures.DTOs;
using Learnify.Application.Sections.DTOs;
using Learnify.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Learnify.Application.Courses.Queries.GetCourseWithContent
{
    public class GetCourseWithContentQueryHandler : IRequestHandler<GetCourseWithContentQuery, CourseWithContentResponse>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly ILectureRepository _lectureRepository;
        private readonly IMapper _mapper;

        public GetCourseWithContentQueryHandler(
            ICourseRepository courseRepository,
            ISectionRepository sectionRepository, 
            ILectureRepository lectureRepository,
            IMapper mapper)
        {
            _courseRepository = courseRepository;
            _sectionRepository = sectionRepository;
            _lectureRepository = lectureRepository;
            _mapper = mapper;
        }

        public async Task<CourseWithContentResponse> Handle(GetCourseWithContentQuery request, CancellationToken cancellationToken)
        {
            // Get course
            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null)
            {
                throw new NotFoundException(nameof(Course), request.CourseId);
            }

            // Get sections for the course
            var sections = await _sectionRepository.GetByCourseIdAsync(request.CourseId);
            
            var response = _mapper.Map<CourseWithContentResponse>(course);
            response.Sections = new List<SectionWithLecturesResponse>();
            
            double totalDurationSeconds = 0;
            int totalLectures = 0;

            // For each section, get its lectures
            foreach (var section in sections.OrderBy(s => s.Order))
            {
                var lectures = await _lectureRepository.GetBySectionIdAsync(section.Id);
                var lectureResponses = _mapper.Map<List<LectureResponse>>(lectures.OrderBy(l => l.Order));
                
                // Calculate section duration and set lecture properties
                double sectionDurationSeconds = 0;
                foreach (var lectureResponse in lectureResponses)
                {
                    // Compute IsFree: lecture preview OR section is free preview
                    lectureResponse.IsFree = lectureResponse.IsPreviewAllowed || section.IsFreePreview;
                    
                    // Format duration as MM:SS
                    lectureResponse.FormattedDuration = FormatDuration(lectureResponse.DurationInSeconds);
                    
                    sectionDurationSeconds += lectureResponse.DurationInSeconds;
                }
                
                totalDurationSeconds += sectionDurationSeconds;
                totalLectures += lectureResponses.Count;

                var sectionWithLectures = _mapper.Map<SectionWithLecturesResponse>(section);
                sectionWithLectures.Lectures = lectureResponses;
                sectionWithLectures.DurationSeconds = sectionDurationSeconds;
                sectionWithLectures.DurationMinutes = (int)Math.Ceiling(sectionDurationSeconds / 60.0);
                sectionWithLectures.FormattedDuration = FormatDuration(sectionDurationSeconds);
                sectionWithLectures.LecturesCount = lectureResponses.Count;
                
                response.Sections.Add(sectionWithLectures);
            }

            // Set calculated totals
            response.TotalDurationMinutes = (int)Math.Ceiling(totalDurationSeconds / 60.0);
            response.TotalLecturesCount = totalLectures;
            response.SectionsCount = response.Sections.Count;

            return response;
        }
        
        private static string FormatDuration(double totalSeconds)
        {
            if (totalSeconds <= 0) return "--:--";
            
            var timeSpan = TimeSpan.FromSeconds(totalSeconds);
            if (timeSpan.TotalHours >= 1)
            {
                return $"{(int)timeSpan.TotalHours}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            }
            return $"{timeSpan.Minutes}:{timeSpan.Seconds:D2}";
        }
    }
}
 