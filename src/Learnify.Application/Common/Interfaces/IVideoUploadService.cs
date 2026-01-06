using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using Learnify.Application.Common.Models;

namespace Learnify.Application.Common.Interfaces
{
    public interface IVideoUploadService
    {
        Task<string> UploadVideoAsync(IFormFile videoFile, string lectureId);
        Task<VideoUploadResult> UploadVideoWithDurationAsync(IFormFile videoFile, string lectureId);
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
        Task<bool> DeleteVideoAsync(string videoUrl);
    }
} 