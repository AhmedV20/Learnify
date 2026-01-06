using System.ComponentModel.DataAnnotations;

namespace Learnify.Application.Lectures.DTOs.Requests
{
    public class ToggleLecturePreviewRequest
    {
        [Required]
        public bool IsPreviewAllowed { get; set; }
    }
} 