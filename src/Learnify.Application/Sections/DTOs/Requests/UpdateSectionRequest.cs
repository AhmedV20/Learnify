using System.ComponentModel.DataAnnotations;

namespace Learnify.Application.Sections.DTOs.Requests;

public class UpdateSectionRequest
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    public int Order { get; set; }
} 