namespace Learnify.Api.RateLimiting;

/// <summary>
/// Rate limiting policy names for different endpoint groups
/// </summary>
public static class RateLimitPolicies
{
    /// <summary>
    /// Authentication endpoints (login, register, forgot password)
    /// Limit: 5 requests per 15 minutes
    /// </summary>
    public const string Auth = "auth";

    /// <summary>
    /// OTP/Verification endpoints
    /// Limit: 3 requests per 5 minutes
    /// </summary>
    public const string Otp = "otp";

    /// <summary>
    /// General API endpoints (authenticated)
    /// Limit: 100 requests per minute
    /// </summary>
    public const string General = "general";

    /// <summary>
    /// File upload endpoints
    /// Limit: 10 requests per minute
    /// </summary>
    public const string FileUpload = "file_upload";

    /// <summary>
    /// Payment endpoints (checkout, submit)
    /// Limit: 10 requests per minute
    /// </summary>
    public const string Payment = "payment";

    /// <summary>
    /// Search endpoints (course search, filtering)
    /// Limit: 30 requests per minute
    /// </summary>
    public const string Search = "search";

    /// <summary>
    /// Admin endpoints
    /// Limit: 50 requests per minute
    /// </summary>
    public const string Admin = "admin";
}
