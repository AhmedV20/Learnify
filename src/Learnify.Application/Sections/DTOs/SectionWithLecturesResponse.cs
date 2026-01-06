using Learnify.Application.Lectures.DTOs;
using System.Collections.Generic;

namespace Learnify.Application.Sections.DTOs
{
    public class SectionWithLecturesResponse
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsFreePreview { get; set; }
        public int Order { get; set; }
        
        // Calculated fields
        public double DurationSeconds { get; set; }
        public int DurationMinutes { get; set; }
        public string FormattedDuration { get; set; } = "--:--";
        public int LecturesCount { get; set; }
        
        // Nested lectures
        public List<LectureResponse> Lectures { get; set; } = new List<LectureResponse>();
    }
}
 