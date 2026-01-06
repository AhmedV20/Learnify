using System;
using System.ComponentModel.DataAnnotations;

namespace Learnify.Domain.Entities;

/// <summary>
/// System-wide settings stored as key-value pairs.
/// Used for feature toggles and configuration.
/// </summary>
public class SystemSetting
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Setting key (e.g., "ManualPaymentEnabled")
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Setting value (e.g., "true", "false", or any string)
    /// </summary>
    [Required]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of what this setting does
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
