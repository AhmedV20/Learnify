using AutoMapper;
using Learnify.Application.Sections.Commands.CreateSection;
using Learnify.Application.Sections.Commands.UpdateSection;
using Learnify.Application.Sections.DTOs;
using Learnify.Application.Sections.DTOs.Requests;
using Learnify.Domain.Entities;

namespace Learnify.Application.Common.Mappings;

public class SectionMappingProfile : Profile
{
    public SectionMappingProfile()
    {
        CreateMap<Section, SectionResponse>();
        CreateMap<CreateSectionRequest, CreateSectionCommand>();
        CreateMap<UpdateSectionRequest, UpdateSectionCommand>();
    }
} 