using AutoMapper;
using Learnify.Application.Categories.Commands.UpdateCategory;
using Learnify.Domain.Entities;
using Learnify.Application.Categories.Commands.CreateCategory;
using Learnify.Application.Categories.DTOs.Requests;
using Learnify.Application.Categories.DTOs.Responses;

namespace Learnify.Api.Mappings;

public class CategoryMappings : Profile
{
    public CategoryMappings()
    {
        // Domain to Response DTO
        CreateMap<Category, CategoryResponse>();
        CreateMap<CreateCategoryCommand, Category>();


        // Request DTO to Command
        CreateMap<CreateCategoryRequest, CreateCategoryCommand>();
        
        // For UpdateCourseCommand, we need to handle the Id parameter
        CreateMap<(UpdateCategoryRequest Request, int Id), UpdateCategoryCommand>()
            .ConstructUsing(src => new UpdateCategoryCommand(src.Id, src.Request.Name, src.Request.Slug));
    }
} 