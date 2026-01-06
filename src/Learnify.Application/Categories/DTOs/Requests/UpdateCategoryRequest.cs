using System.ComponentModel.DataAnnotations;

namespace Learnify.Application.Categories.DTOs.Requests;
public class UpdateCategoryRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Slug { get; set; } = string.Empty;
}