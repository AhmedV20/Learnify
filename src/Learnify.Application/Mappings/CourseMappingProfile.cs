using AutoMapper;
using Learnify.Application.Courses.Commands.CreateCourse;
using Learnify.Application.Courses.Commands.UpdateCourse;
using Learnify.Application.Courses.DTOs;
using Learnify.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Application.Mappings
{
    public class CourseMappingProfile : Profile
    {
        public CourseMappingProfile()
        {
            CreateMap<Course, CourseResponse>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
                .ForMember(dest => dest.InstructorName, opt => opt.MapFrom(src => src.Instructor != null ? src.Instructor.UserName : null))
                .ForMember(dest => dest.ThumbnailImageUrl, opt => opt.MapFrom<ThumbnailUrlResolver>());

            CreateMap<CreateCourseCommand, Course>();
            CreateMap<UpdateCourseCommand, Course>();
            
            // New mapping for course with content
            CreateMap<Course, CourseWithContentResponse>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
                .ForMember(dest => dest.InstructorName, opt => opt.MapFrom(src => src.Instructor != null ? src.Instructor.UserName : null))
                .ForMember(dest => dest.ThumbnailImageUrl, opt => opt.MapFrom<ThumbnailUrlResolverWithContent>())
                .ForMember(dest => dest.Sections, opt => opt.Ignore()) // Will be mapped manually in handler
                .ForMember(dest => dest.TotalDurationMinutes, opt => opt.Ignore())
                .ForMember(dest => dest.TotalLecturesCount, opt => opt.Ignore())
                .ForMember(dest => dest.SectionsCount, opt => opt.Ignore());
        }
    }
}
