namespace Learnify.Application.Lectures.DTOs
{
    public class LectureResponse
    {
        public int Id { get; set; }
        public int SectionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
        
        public double DurationInSeconds { get; set; }
        public int? DurationInMinutes { get; set; }
        public string FormattedDuration { get; set; } = "--:--";  // e.g. "12:34"
        
        public int Order { get; set; }
        public bool IsPreviewAllowed { get; set; }
        public bool IsFree { get; set; }  // Computed: IsPreviewAllowed OR Section.IsFreePreview
    }
}
 