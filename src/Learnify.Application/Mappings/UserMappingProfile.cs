using AutoMapper;
using Learnify.Application.Users.DTOs.Response;
using Learnify.Domain.Entities;

namespace Learnify.Application.Mappings
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<ApplicationUser, UserProfileResponse>()
                .ForMember(dest => dest.ProfilePictureUrl, opt => opt.MapFrom(src => src.ProfileImageUrl));
        }
    }
} 