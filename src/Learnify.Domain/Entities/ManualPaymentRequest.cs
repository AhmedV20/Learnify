using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learnify.Domain.Entities;

/// <summary>
/// Manual payment request submitted by user with proof screenshot.
/// Admin reviews and approves/rejects.
/// </summary>
public class ManualPaymentRequest
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// User who submitted the payment
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;
    public virtual ApplicationUser? User { get; set; }

    /// <summary>
    /// Payment method used
    /// </summary>
    [Required]
    public int PaymentMethodId { get; set; }
    public virtual ManualPaymentMethod? PaymentMethod { get; set; }

    /// <summary>
    /// Total amount paid
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    /// <summary>
    /// URL to payment proof screenshot (stored in Cloudinary)
    /// </summary>
    [Required]
    [StringLength(500)]
    public string ProofImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// Optional message from user
    /// </summary>
    public string? UserMessage { get; set; }

    /// <summary>
    /// Payment status: Pending, Approved, Rejected
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// Admin's note/reason (shown to user on approval/rejection)
    /// </summary>
    public string? AdminNote { get; set; }

    /// <summary>
    /// Admin who reviewed this request
    /// </summary>
    public string? ReviewedByAdminId { get; set; }
    public virtual ApplicationUser? ReviewedByAdmin { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }

    // Navigation properties - courses being purchased
    public virtual ICollection<ManualPaymentCartItem> CartItems { get; set; } = new List<ManualPaymentCartItem>();
}
