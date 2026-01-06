using MediatR;
using Learnify.Domain.Entities;
using Learnify.Application.Common.Interfaces;

namespace Learnify.Application.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Category>
{
    private readonly ICategoriesRepository _categoriesRepository;

    public UpdateCategoryCommandHandler(ICategoriesRepository categoriesRepository)
    {
        _categoriesRepository = categoriesRepository;
    }

    public async Task<Category> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoriesRepository.GetCategoryByIdAsync(request.Id);
        
        if (category == null)
        {
            throw new KeyNotFoundException($"Category with ID {request.Id} not found.");
        }

        category.Name = request.Name;
        category.Slug = request.Slug;

        await _categoriesRepository.UpdateCategoryAsync(category);

        return category;
    }
} 