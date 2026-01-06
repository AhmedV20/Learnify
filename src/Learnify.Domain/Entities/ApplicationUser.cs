using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Learnify.Domain.Entities;
public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModifiedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ProfileImageUrl { get; set; }
    public string? StripeConnectAccountId { get; set; } // For instructor payouts via Stripe Connect
    
    // OAuth Provider fields
    public string? GoogleId { get; set; } // Google unique identifier (sub claim)
    public string? AuthProvider { get; set; } // "Local", "Google", "Facebook", etc.
    public bool IsCustomProfilePic { get; set; } = false; // True if user uploaded custom pic via API
    
    // Email verification OTP fields
    public string? EmailOtpHash { get; set; } // SHA256 hashed OTP
    public DateTime? EmailOtpExpiry { get; set; } // OTP expiration time
    public int EmailOtpAttempts { get; set; } = 0; // Failed attempts counter (max 5)
    
    // Password reset OTP fields
    public string? PasswordResetOtpHash { get; set; } // SHA256 hashed OTP
    public DateTime? PasswordResetOtpExpiry { get; set; } // OTP expiration time
    public int PasswordResetOtpAttempts { get; set; } = 0; // Failed attempts counter (max 5)
    public string? PasswordResetTokenHash { get; set; } // Token returned after OTP verification
    public DateTime? PasswordResetTokenExpiry { get; set; } // Token expiration (10 min)

    // Two-Factor Authentication fields
    public bool TwoFactorAuthEnabled { get; set; } = false;
    public Enums.TwoFactorMethod? TwoFactorMethod { get; set; }
    public string? TwoFactorSecretKey { get; set; } // TOTP secret key (encrypted)
    public string? BackupCodesHash { get; set; } // JSON array of SHA256 hashed backup codes
    public int BackupCodesRemaining { get; set; } = 0;
    
    // 2FA Login OTP fields (for Email method)
    public string? TwoFactorOtpHash { get; set; }
    public DateTime? TwoFactorOtpExpiry { get; set; }
    public int TwoFactorOtpAttempts { get; set; } = 0;
    
    // 2FA Login session token
    public string? TwoFactorTokenHash { get; set; }
    public DateTime? TwoFactorTokenExpiry { get; set; }

    // Navigation properties
    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public virtual ICollection<CourseRating> CourseRatings { get; set; } = new List<CourseRating>();
    public virtual ICollection<UserBookmark> Bookmarks { get; set; } = new List<UserBookmark>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public virtual ICollection<AppliedCoupon> AppliedCoupons { get; set; } = new List<AppliedCoupon>();
}
