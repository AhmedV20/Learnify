using MediatR;
using Microsoft.AspNetCore.Http;

namespace Learnify.Application.Lectures.Commands.UploadVideo
{
    public record UploadVideoCommand : IRequest<UploadVideoResponse>
    {
        public int LectureId { get; set; }
        public IFormFile VideoFile { get; set; } = null!;
    }
} 