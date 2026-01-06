using FluentValidation;
using Learnify.Application.Common.Validation;
using Microsoft.AspNetCore.Http;

namespace Learnify.Application.Courses.DTOs.Validators;

public class CreateCourseRequestValidator : AbstractValidator<CreateCourseRequest>
{
    public CreateCourseRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Course title is required")
            .MinimumLength(ValidationConstants.Course.TitleMinLength)
                .WithMessage($"Title must be at least {ValidationConstants.Course.TitleMinLength} characters")
            .MaximumLength(ValidationConstants.Course.TitleMaxLength)
                .WithMessage($"Title cannot exceed {ValidationConstants.Course.TitleMaxLength} characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MinimumLength(ValidationConstants.Course.DescriptionMinLength)
                .WithMessage($"Description must be at least {ValidationConstants.Course.DescriptionMinLength} characters")
            .MaximumLength(ValidationConstants.Course.DescriptionMaxLength)
                .WithMessage($"Description cannot exceed {ValidationConstants.Course.DescriptionMaxLength} characters");

        RuleFor(x => x.Price)
            .InclusiveBetween(ValidationConstants.Course.MinPrice, ValidationConstants.Course.MaxPrice)
                .WithMessage($"Price must be between {ValidationConstants.Course.MinPrice} and {ValidationConstants.Course.MaxPrice}");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Please select a valid category")
            .When(x => x.CategoryId.HasValue);

        RuleFor(x => x.ThumbnailFile)
            .Must(BeValidImageFile).WithMessage("Thumbnail must be a valid image file (JPEG, PNG, WebP)")
            .Must(BeUnderMaxSize).WithMessage("Thumbnail file size cannot exceed 5MB")
            .When(x => x.ThumbnailFile != null);
    }

    private bool BeValidImageFile(IFormFile? file)
    {
        if (file == null) return true;
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/jpg" };
        return allowedTypes.Contains(file.ContentType.ToLowerInvariant());
    }

    private bool BeUnderMaxSize(IFormFile? file)
    {
        if (file == null) return true;
        const int maxSizeInBytes = 5 * 1024 * 1024; // 5MB
        return file.Length <= maxSizeInBytes;
    }
}