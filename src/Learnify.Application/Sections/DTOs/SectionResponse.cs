namespace Learnify.Application.Sections.DTOs;

public class SectionResponse
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsFreePreview { get; set; }
    public int Order { get; set; }
}
 