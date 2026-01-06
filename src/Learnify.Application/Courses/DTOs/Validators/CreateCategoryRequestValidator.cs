using FluentValidation;
using Learnify.Application.Categories.DTOs.Requests;
using Learnify.Application.Common.Interfaces;
using Learnify.Application.Common.Validation;

namespace Learnify.Application.Categories.DTOs.Validators;

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator(ICategoriesRepository categoryRepository)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required")
            .MinimumLength(ValidationConstants.Category.NameMinLength)
                .WithMessage($"Name must be at least {ValidationConstants.Category.NameMinLength} characters")
            .MaximumLength(ValidationConstants.Category.NameMaxLength)
                .WithMessage($"Name cannot exceed {ValidationConstants.Category.NameMaxLength} characters");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug is required")
            .Matches(ValidationConstants.Category.SlugPattern)
                .WithMessage("Slug must be lowercase, using only letters, numbers, and hyphens")
            .MustAsync(async (slug, cancellation) =>
            {
                var existing = await categoryRepository.GetCategoryBySlugAsync(slug);
                return existing == null;
            }).WithMessage("A category with this slug already exists");
    }
}