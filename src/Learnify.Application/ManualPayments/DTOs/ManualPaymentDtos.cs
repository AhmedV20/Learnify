namespace Learnify.Application.ManualPayments.DTOs;

/// <summary>
/// Payment method details returned to users
/// </summary>
public record ManualPaymentMethodDto(
    int Id,
    string Name,
    string? NameAr,
    string Type,
    string AccountIdentifier,
    string? AccountName,
    string? Instructions,
    string? InstructionsAr,
    string? IconUrl,
    int DisplayOrder
);

/// <summary>
/// Payment method for admin (includes active status)
/// </summary>
public record ManualPaymentMethodAdminDto(
    int Id,
    string Name,
    string? NameAr,
    string Type,
    string AccountIdentifier,
    string? AccountName,
    string? Instructions,
    string? InstructionsAr,
    string? IconUrl,
    bool IsActive,
    int DisplayOrder,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
/// Create/Update payment method request
/// </summary>
public record CreatePaymentMethodRequest(
    string Name,
    string? NameAr,
    string Type,
    string AccountIdentifier,
    string? AccountName,
    string? Instructions,
    string? InstructionsAr,
    string? IconUrl,
    bool IsActive,
    int DisplayOrder
);

/// <summary>
/// Payment request submitted by user
/// </summary>
public record ManualPaymentRequestDto(
    int Id,
    int PaymentMethodId,
    string PaymentMethodName,
    decimal Amount,
    string ProofImageUrl,
    string? UserMessage,
    string Status,
    string? AdminNote,
    DateTime CreatedAt,
    DateTime? ReviewedAt,
    List<ManualPaymentCartItemDto> Items
);

/// <summary>
/// Admin view of payment request (includes user info)
/// </summary>
public record ManualPaymentRequestAdminDto(
    int Id,
    string UserId,
    string UserName,
    string UserEmail,
    int PaymentMethodId,
    string PaymentMethodName,
    decimal Amount,
    string ProofImageUrl,
    string? UserMessage,
    string Status,
    string? AdminNote,
    string? ReviewedByAdminId,
    string? ReviewedByAdminName,
    DateTime CreatedAt,
    DateTime? ReviewedAt,
    List<ManualPaymentCartItemDto> Items
);

/// <summary>
/// Cart item in a payment request
/// </summary>
public record ManualPaymentCartItemDto(
    int CourseId,
    string CourseTitle,
    string? CourseThumbnail,
    decimal Price
);

/// <summary>
/// Submit payment request
/// </summary>
public record SubmitPaymentRequest(
    int PaymentMethodId,
    string ProofImageUrl,
    string? UserMessage
);

/// <summary>
/// Approve/Reject payment request
/// </summary>
public record ReviewPaymentRequest(
    string? AdminNote
);

/// <summary>
/// System settings response
/// </summary>
public record ManualPaymentSettingsDto(
    bool IsEnabled
);
