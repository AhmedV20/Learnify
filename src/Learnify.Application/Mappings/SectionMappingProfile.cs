using AutoMapper;
using Learnify.Application.Sections.Commands.CreateSection;
using Learnify.Application.Sections.Commands.UpdateSection;
using Learnify.Application.Sections.DTOs;
using Learnify.Application.Sections.DTOs.Requests;
using Learnify.Domain.Entities;

namespace Learnify.Application.Mappings
{
    public class SectionMappingProfile : Profile
    {
        public SectionMappingProfile()
        {
            CreateMap<Section, SectionResponse>();
            CreateMap<CreateSectionCommand, Section>();
            CreateMap<CreateSectionRequest, CreateSectionCommand>();

            CreateMap<UpdateSectionRequest, UpdateSectionCommand>();
            CreateMap<UpdateSectionCommand, Section>();
            
            // New mapping for section with lectures
            CreateMap<Section, SectionWithLecturesResponse>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => "")) // Default empty description
                .ForMember(dest => dest.Lectures, opt => opt.Ignore()) // Will be mapped manually in handler
                .ForMember(dest => dest.DurationMinutes, opt => opt.Ignore())
                .ForMember(dest => dest.LecturesCount, opt => opt.Ignore());
        }
    }
} 