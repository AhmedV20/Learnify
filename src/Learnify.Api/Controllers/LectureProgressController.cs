using Asp.Versioning;
using Learnify.Application.Common.Interfaces;
using Learnify.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Learnify.Api.Controllers;

/// <summary>
/// API endpoints for tracking user progress on lectures (resume playback, completion).
/// </summary>
[ApiController]
    [ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class LectureProgressController : ControllerBase
{
    private readonly ILectureProgressRepository _progressRepository;
    private readonly ILogger<LectureProgressController> _logger;

    public LectureProgressController(
        ILectureProgressRepository progressRepository,
        ILogger<LectureProgressController> logger)
    {
        _progressRepository = progressRepository;
        _logger = logger;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

    /// <summary>
    /// Get progress for a specific lecture
    /// </summary>
    [HttpGet("lecture/{lectureId}")]
    public async Task<IActionResult> GetProgress(int lectureId)
    {
        var userId = GetUserId();
        var progress = await _progressRepository.GetAsync(userId, lectureId);
        
        if (progress == null)
        {
            return Ok(new LectureProgressDto
            {
                LectureId = lectureId,
                WatchedSeconds = 0,
                IsCompleted = false
            });
        }

        return Ok(new LectureProgressDto
        {
            LectureId = progress.LectureId,
            WatchedSeconds = progress.WatchedSeconds,
            IsCompleted = progress.IsCompleted,
            LastWatchedAt = progress.LastWatchedAt
        });
    }

    /// <summary>
    /// Get all progress for a course
    /// </summary>
    [HttpGet("course/{courseId}")]
    public async Task<IActionResult> GetCourseProgress(int courseId)
    {
        var userId = GetUserId();
        var progressList = await _progressRepository.GetByCourseAsync(userId, courseId);
        
        var result = progressList.Select(p => new LectureProgressDto
        {
            LectureId = p.LectureId,
            WatchedSeconds = p.WatchedSeconds,
            IsCompleted = p.IsCompleted,
            LastWatchedAt = p.LastWatchedAt
        });

        return Ok(result);
    }

    /// <summary>
    /// Update watch position for a lecture (called periodically during video playback)
    /// </summary>
    [HttpPost("lecture/{lectureId}/update")]
    public async Task<IActionResult> UpdateProgress(int lectureId, [FromBody] UpdateProgressRequest request)
    {
        var userId = GetUserId();
        
        var progress = new LectureProgress
        {
            UserId = userId,
            LectureId = lectureId,
            WatchedSeconds = request.WatchedSeconds,
            IsCompleted = request.IsCompleted
        };

        var result = await _progressRepository.UpsertAsync(progress);
        
        _logger.LogInformation("Updated progress for user {UserId} on lecture {LectureId}: {WatchedSeconds}s, Completed: {IsCompleted}", 
            userId, lectureId, request.WatchedSeconds, request.IsCompleted);

        return Ok(new LectureProgressDto
        {
            LectureId = result.LectureId,
            WatchedSeconds = result.WatchedSeconds,
            IsCompleted = result.IsCompleted,
            LastWatchedAt = result.LastWatchedAt
        });
    }

    /// <summary>
    /// Mark a lecture as completed
    /// </summary>
    [HttpPost("lecture/{lectureId}/complete")]
    public async Task<IActionResult> MarkCompleted(int lectureId)
    {
        var userId = GetUserId();
        await _progressRepository.MarkCompletedAsync(userId, lectureId);
        
        _logger.LogInformation("User {UserId} marked lecture {LectureId} as completed", userId, lectureId);
        
        return Ok(new { message = "Lecture marked as completed" });
    }

    /// <summary>
    /// Mark a lecture as incomplete (undo completion)
    /// </summary>
    [HttpPost("lecture/{lectureId}/incomplete")]
    public async Task<IActionResult> MarkIncomplete(int lectureId)
    {
        var userId = GetUserId();
        await _progressRepository.MarkIncompleteAsync(userId, lectureId);
        
        _logger.LogInformation("User {UserId} marked lecture {LectureId} as incomplete", userId, lectureId);
        
        return Ok(new { message = "Lecture marked as incomplete" });
    }
}

// DTOs
public class LectureProgressDto
{
    public int LectureId { get; set; }
    public double WatchedSeconds { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? LastWatchedAt { get; set; }
}

public class UpdateProgressRequest
{
    public double WatchedSeconds { get; set; }
    public bool IsCompleted { get; set; }
}
