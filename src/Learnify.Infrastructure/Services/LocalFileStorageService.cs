using Learnify.Application.Common.Interfaces;
using Learnify.Application.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Learnify.Infrastructure.Services
{
    /// <summary>
    /// Local file storage service that saves files to the wwwroot/uploads folder.
    /// Use this for development or when Google Cloud credentials are not available.
    /// </summary>
    public class LocalFileStorageService : IVideoUploadService
    {
        private readonly string _uploadPath;
        private readonly string _baseUrl;
        private readonly ILogger<LocalFileStorageService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LocalFileStorageService(
            IConfiguration configuration, 
            ILogger<LocalFileStorageService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            
            // Get configuration values with defaults
            _uploadPath = configuration["LocalStorage:UploadPath"] ?? "wwwroot/uploads";
            _baseUrl = configuration["LocalStorage:BaseUrl"] ?? "/uploads";
            
            // Ensure upload directory exists
            var fullPath = Path.GetFullPath(_uploadPath);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                _logger.LogInformation($"Created upload directory: {fullPath}");
            }
            
            _logger.LogInformation($"LocalFileStorageService initialized. Upload path: {fullPath}");
        }

        /// <summary>
        /// Gets the full URL including scheme and host (e.g., http://localhost:5279/uploads/...)
        /// </summary>
        private string GetFullUrl(string relativePath)
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request != null)
            {
                var scheme = request.Scheme;
                var host = request.Host.ToString();
                return $"{scheme}://{host}{relativePath}";
            }
            // Fallback to relative URL if no HttpContext available
            return relativePath;
        }

        public async Task<string> UploadVideoAsync(IFormFile videoFile, string lectureId)
        {
            try
            {
                // Validate file
                if (videoFile == null || videoFile.Length == 0)
                {
                    throw new ArgumentException("Video file is null or empty");
                }

                // Validate file type
                var allowedTypes = new[] { "video/mp4", "video/avi", "video/mov", "video/wmv", "video/webm", "video/quicktime" };
                if (!Array.Exists(allowedTypes, t => t.Equals(videoFile.ContentType?.ToLower(), StringComparison.OrdinalIgnoreCase)))
                {
                    throw new ArgumentException($"Unsupported video format: {videoFile.ContentType}");
                }

                // Generate unique file name
                var fileExtension = Path.GetExtension(videoFile.FileName);
                var fileName = $"videos/lectures/{lectureId}/{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(_uploadPath, fileName);

                // Ensure directory exists
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                _logger.LogInformation($"Uploading video to local storage: {filePath}");

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await videoFile.CopyToAsync(stream);
                }

                // Return full URL
                var relativePath = $"{_baseUrl}/{fileName.Replace("\\", "/")}";
                var url = GetFullUrl(relativePath);
                _logger.LogInformation($"Video uploaded successfully: {url}");
                
                return url;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading video to local storage");
                throw;
            }
        }

        public async Task<VideoUploadResult> UploadVideoWithDurationAsync(IFormFile videoFile, string lectureId)
        {
            try
            {
                // Validate file
                if (videoFile == null || videoFile.Length == 0)
                {
                    throw new ArgumentException("Video file is null or empty");
                }

                // Validate file type
                var allowedTypes = new[] { "video/mp4", "video/avi", "video/mov", "video/wmv", "video/webm", "video/quicktime" };
                if (!Array.Exists(allowedTypes, t => t.Equals(videoFile.ContentType?.ToLower(), StringComparison.OrdinalIgnoreCase)))
                {
                    throw new ArgumentException($"Unsupported video format: {videoFile.ContentType}");
                }

                _logger.LogInformation($"Starting video upload with duration extraction for lecture {lectureId}");

                // Generate unique file name
                var fileExtension = Path.GetExtension(videoFile.FileName);
                var fileName = $"videos/lectures/{lectureId}/{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(_uploadPath, fileName);

                // Ensure directory exists
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await videoFile.CopyToAsync(stream);
                }

                // Try to extract duration using FFMpegCore if available
                int durationInMinutes = 0;
                double durationInSeconds = 0;

                try
                {
                    var mediaInfo = await FFMpegCore.FFProbe.AnalyseAsync(filePath);
                    durationInSeconds = mediaInfo.Duration.TotalSeconds;
                    durationInMinutes = (int)Math.Ceiling(mediaInfo.Duration.TotalMinutes);
                    _logger.LogInformation($"Video duration extracted: {durationInSeconds:F2} seconds ({durationInMinutes} minutes)");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to extract video duration, defaulting to 0. FFMpeg may not be installed.");
                }

                // Return full URL
                var relativePath = $"{_baseUrl}/{fileName.Replace("\\", "/")}";
                var url = GetFullUrl(relativePath);
                _logger.LogInformation($"Video uploaded successfully with duration: {url}");

                return new VideoUploadResult
                {
                    VideoUrl = url,
                    DurationInMinutes = durationInMinutes,
                    DurationInSeconds = durationInSeconds
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading video with duration extraction to local storage");
                throw;
            }
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            try
            {
                // Validate parameters
                if (fileStream == null || fileStream.Length == 0)
                {
                    throw new ArgumentException("File stream is null or empty");
                }

                if (string.IsNullOrEmpty(fileName))
                {
                    throw new ArgumentException("File name cannot be null or empty");
                }

                var filePath = Path.Combine(_uploadPath, fileName);

                // Ensure directory exists
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                _logger.LogInformation($"Uploading file to local storage: {filePath}");

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await fileStream.CopyToAsync(stream);
                }

                // Return full URL
                var relativePath = $"{_baseUrl}/{fileName.Replace("\\", "/")}";
                var url = GetFullUrl(relativePath);
                _logger.LogInformation($"File uploaded successfully: {url}");
                
                return url;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file to local storage");
                throw;
            }
        }

        public Task<bool> DeleteVideoAsync(string videoUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(videoUrl))
                {
                    return Task.FromResult(true); // Nothing to delete
                }

                // Convert URL to file path
                var relativePath = videoUrl.Replace(_baseUrl, "").TrimStart('/');
                var filePath = Path.Combine(_uploadPath, relativePath);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation($"Video deleted successfully: {filePath}");
                }
                else
                {
                    _logger.LogWarning($"Video file not found for deletion: {filePath}");
                }

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting video from local storage");
                return Task.FromResult(false);
            }
        }
    }
}
