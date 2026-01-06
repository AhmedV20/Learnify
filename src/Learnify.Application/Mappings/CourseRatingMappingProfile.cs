using AutoMapper;
using Learnify.Application.CourseRatings.Commands.AddCourseRating;
using Learnify.Application.CourseRatings.DTOs.Responses;
using Learnify.Domain.Entities;

namespace Learnify.Application.Mappings
{
    public class CourseRatingMappingProfile : Profile
    {
        public CourseRatingMappingProfile()
        {
            CreateMap<AddCourseRatingCommand, CourseRating>();
            CreateMap<CourseRating, CourseRatingResponse>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.UserProfileImageUrl, opt => opt.MapFrom(src => src.User.ProfileImageUrl));
        }
    }
} 