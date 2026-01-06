using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learnify.Domain.Entities;

/// <summary>
/// Tracks user's progress for each lecture - resume position and completion status.
/// </summary>
public class LectureProgress
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [ForeignKey("UserId")]
    public virtual ApplicationUser? User { get; set; }
    
    [Required]
    public int LectureId { get; set; }
    
    [ForeignKey("LectureId")]
    public virtual Lecture? Lecture { get; set; }
    
    /// <summary>
    /// How many seconds the user has watched (for resume playback).
    /// </summary>
    public double WatchedSeconds { get; set; }
    
    /// <summary>
    /// Whether the user has marked this lecture as completed.
    /// </summary>
    public bool IsCompleted { get; set; }
    
    public DateTime LastWatchedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
