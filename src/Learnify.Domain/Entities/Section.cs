using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learnify.Domain.Entities;

public class Section
{
    [Key]
    public int Id { get; set; } 

    [Required]
    public int CourseId { get; set; } // FK to Course
    public virtual Course? Course { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    public bool IsFreePreview { get; set; } = false;  // Makes ALL lectures in section free
    
    public int Order { get; set; }

    // Navigation property
    public virtual ICollection<Lecture> Lectures { get; set; } = new List<Lecture>();
}
