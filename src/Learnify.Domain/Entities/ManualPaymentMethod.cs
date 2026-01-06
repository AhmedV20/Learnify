using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Learnify.Domain.Entities;

/// <summary>
/// Configurable manual payment method (InstaPay, VodafoneCash, Bank, etc.)
/// Admin can add/edit/delete these from the dashboard.
/// </summary>
public class ManualPaymentMethod
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Display name in English (e.g., "InstaPay", "VodafoneCash")
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Display name in Arabic (e.g., "انستاباي")
    /// </summary>
    [StringLength(100)]
    public string? NameAr { get; set; }

    /// <summary>
    /// Type identifier (e.g., "instapay", "vodafonecash", "bank")
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Account identifier (e.g., "@ahmed" for InstaPay, "01012345678" for VodafoneCash)
    /// </summary>
    [Required]
    [StringLength(255)]
    public string AccountIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// Account holder name
    /// </summary>
    [StringLength(255)]
    public string? AccountName { get; set; }

    /// <summary>
    /// Instructions in English (how to send payment)
    /// </summary>
    public string? Instructions { get; set; }

    /// <summary>
    /// Instructions in Arabic
    /// </summary>
    public string? InstructionsAr { get; set; }

    /// <summary>
    /// Icon URL (optional, stored in Cloudinary)
    /// </summary>
    [StringLength(500)]
    public string? IconUrl { get; set; }

    /// <summary>
    /// Whether this payment method is active and visible to users
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Display order (lower = first)
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<ManualPaymentRequest> PaymentRequests { get; set; } = new List<ManualPaymentRequest>();
}
