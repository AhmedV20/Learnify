using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using Learnify.Api.RateLimiting;
using Learnify.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Learnify.Api.Controllers;

/// <summary>
/// Controller for file upload operations
/// </summary>
[ApiController]
    [ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[EnableRateLimiting(RateLimitPolicies.FileUpload)]
public class FilesController : ControllerBase
{
    private readonly IVideoUploadService _uploadService;
    private readonly ILogger<FilesController> _logger;
    
    private static readonly HashSet<string> AllowedImageTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/gif",
        "image/webp"
    };
    
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

    public FilesController(
        IVideoUploadService uploadService,
        ILogger<FilesController> logger)
    {
        _uploadService = uploadService;
        _logger = logger;
    }

    /// <summary>
    /// Upload an image file (for payment proofs, etc.)
    /// </summary>
    [HttpPost("upload")]
    [ProducesResponseType(typeof(FileUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "No file provided" });
        }

        // Validate file size
        if (file.Length > MaxFileSize)
        {
            return BadRequest(new { error = "File size must be less than 5MB" });
        }

        // Validate file type
        if (!AllowedImageTypes.Contains(file.ContentType))
        {
            return BadRequest(new { error = "Only image files (JPEG, PNG, GIF, WebP) are allowed" });
        }

        try
        {
            // Generate unique filename
            var extension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"payments/{Guid.NewGuid()}{extension}";

            // Upload file using Cloudinary (or configured storage service)
            using var stream = file.OpenReadStream();
            var url = await _uploadService.UploadFileAsync(stream, uniqueFileName, file.ContentType);

            _logger.LogInformation("File uploaded successfully to Cloudinary: {Url}", url);

            return Ok(new FileUploadResponse { Url = url });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to Cloudinary");
            return StatusCode(500, new { error = "Failed to upload file" });
        }
    }
}

/// <summary>
/// Response for file upload
/// </summary>
public class FileUploadResponse
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}
