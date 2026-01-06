using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Learnify.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

// Use alias to avoid conflict with CloudinaryDotNet.Actions.VideoUploadResult
using AppVideoUploadResult = Learnify.Application.Common.Models.VideoUploadResult;

namespace Learnify.Infrastructure.Services
{
    /// <summary>
    /// Cloudinary storage service for uploading videos and images.
    /// Returns secure HTTPS URLs that are stored directly in the database.
    /// </summary>
    public class CloudinaryStorageService : IVideoUploadService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryStorageService> _logger;

        public CloudinaryStorageService(
            IOptions<CloudinarySettings> config,
            ILogger<CloudinaryStorageService> logger)
        {
            _logger = logger;

            var settings = config.Value;
            
            if (string.IsNullOrEmpty(settings.CloudName) || 
                string.IsNullOrEmpty(settings.ApiKey) || 
                string.IsNullOrEmpty(settings.ApiSecret))
            {
                throw new InvalidOperationException("Cloudinary settings (CloudName, ApiKey, ApiSecret) are not configured properly.");
            }

            var account = new Account(
                settings.CloudName,
                settings.ApiKey,
                settings.ApiSecret
            );

            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true; // Always use HTTPS

            _logger.LogInformation("CloudinaryStorageService initialized for cloud: {CloudName}", settings.CloudName);
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

                _logger.LogInformation("Uploading video to Cloudinary for lecture {LectureId}", lectureId);

                using var stream = videoFile.OpenReadStream();
                
                var uploadParams = new VideoUploadParams
                {
                    File = new FileDescription(videoFile.FileName, stream),
                    PublicId = $"lectures/{lectureId}/{Guid.NewGuid()}",
                    Folder = "Learnify/videos"
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    _logger.LogError("Cloudinary upload failed: {Error}", uploadResult.Error?.Message);
                    throw new Exception($"Video upload failed: {uploadResult.Error?.Message}");
                }

                var secureUrl = uploadResult.SecureUrl.ToString();
                _logger.LogInformation("Video uploaded successfully to Cloudinary: {Url}", secureUrl);

                return secureUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading video to Cloudinary");
                throw;
            }
        }

        public async Task<AppVideoUploadResult> UploadVideoWithDurationAsync(IFormFile videoFile, string lectureId)
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

                _logger.LogInformation("Uploading video with duration extraction to Cloudinary for lecture {LectureId}", lectureId);

                using var stream = videoFile.OpenReadStream();

                var uploadParams = new VideoUploadParams
                {
                    File = new FileDescription(videoFile.FileName, stream),
                    PublicId = $"lectures/{lectureId}/{Guid.NewGuid()}",
                    Folder = "Learnify/videos"
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    _logger.LogError("Cloudinary upload failed: {Error}", uploadResult.Error?.Message);
                    throw new Exception($"Video upload failed: {uploadResult.Error?.Message}");
                }

                // Cloudinary returns duration as double (seconds)
                var durationInSeconds = uploadResult.Duration;
                var durationInMinutes = durationInSeconds > 0 ? (int)Math.Ceiling(durationInSeconds / 60.0) : 0;

                var secureUrl = uploadResult.SecureUrl.ToString();
                _logger.LogInformation(
                    "Video uploaded successfully to Cloudinary: {Url}, Duration: {Duration}s", 
                    secureUrl, 
                    durationInSeconds);

                return new AppVideoUploadResult
                {
                    VideoUrl = secureUrl,
                    DurationInMinutes = durationInMinutes,
                    DurationInSeconds = durationInSeconds
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading video with duration to Cloudinary");
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

                _logger.LogInformation("Uploading file to Cloudinary: {FileName}", fileName);

                // Determine if it's a video or image based on content type
                var isVideo = contentType?.StartsWith("video/", StringComparison.OrdinalIgnoreCase) ?? false;

                if (isVideo)
                {
                    var uploadParams = new VideoUploadParams
                    {
                        File = new FileDescription(fileName, fileStream),
                        PublicId = $"files/{Guid.NewGuid()}",
                        Folder = "Learnify/files"
                    };

                    var result = await _cloudinary.UploadAsync(uploadParams);
                    
                    if (result.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        throw new Exception($"File upload failed: {result.Error?.Message}");
                    }

                    return result.SecureUrl.ToString();
                }
                else
                {
                    // Image upload
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(fileName, fileStream),
                        PublicId = $"images/{Guid.NewGuid()}",
                        Folder = "Learnify/images",
                        Transformation = new Transformation()
                            .Quality("auto")
                            .FetchFormat("auto") // Auto-convert to WebP/AVIF for browsers
                    };

                    var result = await _cloudinary.UploadAsync(uploadParams);

                    if (result.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        throw new Exception($"File upload failed: {result.Error?.Message}");
                    }

                    var secureUrl = result.SecureUrl.ToString();
                    _logger.LogInformation("File uploaded successfully to Cloudinary: {Url}", secureUrl);

                    return secureUrl;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file to Cloudinary");
                throw;
            }
        }

        public async Task<bool> DeleteVideoAsync(string videoUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(videoUrl))
                {
                    return true; // Nothing to delete
                }

                // Extract public ID from Cloudinary URL
                // URL format: https://res.cloudinary.com/{cloud}/video/upload/v{version}/{public_id}.{format}
                var publicId = ExtractPublicIdFromUrl(videoUrl);

                if (string.IsNullOrEmpty(publicId))
                {
                    _logger.LogWarning("Could not extract public ID from URL: {Url}", videoUrl);
                    return false;
                }

                _logger.LogInformation("Deleting video from Cloudinary: {PublicId}", publicId);

                var isVideo = videoUrl.Contains("/video/upload/");
                var deleteParams = new DeletionParams(publicId)
                {
                    ResourceType = isVideo ? ResourceType.Video : ResourceType.Image
                };

                var result = await _cloudinary.DestroyAsync(deleteParams);

                if (result.StatusCode == System.Net.HttpStatusCode.OK && result.Result == "ok")
                {
                    _logger.LogInformation("Video deleted successfully from Cloudinary: {PublicId}", publicId);
                    return true;
                }

                _logger.LogWarning("Failed to delete video from Cloudinary: {Error}", result.Error?.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting video from Cloudinary");
                return false;
            }
        }

        /// <summary>
        /// Extracts the public ID from a Cloudinary URL.
        /// </summary>
        private string ExtractPublicIdFromUrl(string url)
        {
            try
            {
                // Example URL: https://res.cloudinary.com/demo/video/upload/v1234567890/Learnify/videos/lectures/123/abc.mp4
                var uri = new Uri(url);
                var path = uri.AbsolutePath;

                // Find the upload part and extract everything after it
                var uploadIndex = path.IndexOf("/upload/");
                if (uploadIndex == -1) return string.Empty;

                var afterUpload = path.Substring(uploadIndex + 8); // Skip "/upload/"

                // Remove version prefix (v1234567890/)
                if (afterUpload.StartsWith("v") && afterUpload.Contains("/"))
                {
                    afterUpload = afterUpload.Substring(afterUpload.IndexOf("/") + 1);
                }

                // Remove file extension
                var lastDotIndex = afterUpload.LastIndexOf('.');
                if (lastDotIndex > 0)
                {
                    afterUpload = afterUpload.Substring(0, lastDotIndex);
                }

                return afterUpload;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
