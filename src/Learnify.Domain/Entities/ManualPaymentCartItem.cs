using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learnify.Domain.Entities;

/// <summary>
/// Courses included in a manual payment request.
/// Created from cart items when user submits payment proof.
/// </summary>
public class ManualPaymentCartItem
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Parent payment request
    /// </summary>
    [Required]
    public int ManualPaymentRequestId { get; set; }
    public virtual ManualPaymentRequest? PaymentRequest { get; set; }

    /// <summary>
    /// Course being purchased
    /// </summary>
    [Required]
    public int CourseId { get; set; }
    public virtual Course? Course { get; set; }

    /// <summary>
    /// Price at time of purchase
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }
}
