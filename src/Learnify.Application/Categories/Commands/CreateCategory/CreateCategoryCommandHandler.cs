using MediatR;
using Learnify.Domain.Entities;
using Learnify.Application.Common.Interfaces;
using Learnify.Application.Categories.DTOs.Responses;
using AutoMapper;

namespace Learnify.Application.Categories.Commands.CreateCategory;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Category>
{
    private readonly ICategoriesRepository _categoriesRepository;
    private readonly IMapper _mapper;

    public CreateCategoryCommandHandler(ICategoriesRepository categoriesRepository, IMapper mapper)
    {
        _categoriesRepository = categoriesRepository;
        _mapper = mapper;
    }
    public async Task<Category> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        // Create category
        var category = _mapper.Map<Category>(request);

        // Add it to the db
        await _categoriesRepository.AddCategoryAsync(category);
        // Return category

        return category;
    }
}
