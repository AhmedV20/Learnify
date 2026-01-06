using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace Learnify.Api.RateLimiting;

/// <summary>
/// Extension methods for configuring rate limiting
/// </summary>
public static class RateLimitingExtensions
{
    /// <summary>
    /// Adds rate limiting services with predefined policies (partitioned by IP + device)
    /// </summary>
    public static IServiceCollection AddRateLimitingPolicies(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Configure rejection response
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.ContentType = "application/json";
                
                // Get retry-after metadata
                var retryAfterSeconds = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterValue)
                    ? retryAfterValue.TotalSeconds
                    : 60;

                var totalSeconds = (int)Math.Ceiling(retryAfterSeconds);
                var minutes = totalSeconds / 60;
                var seconds = totalSeconds % 60;
                var retryAtUtc = DateTime.UtcNow.AddSeconds(retryAfterSeconds);

                // Set standard headers
                context.HttpContext.Response.Headers.RetryAfter = totalSeconds.ToString();
                context.HttpContext.Response.Headers["X-RateLimit-Reset"] = retryAtUtc.ToString("o");

                // Get client identifier for response
                var clientId = GetClientIdentifier(context.HttpContext);
                var policyName = context.HttpContext.GetEndpoint()?.Metadata.GetMetadata<EnableRateLimitingAttribute>()?.PolicyName ?? "global";

                // Build human-readable wait time
                var waitTimeMessage = BuildWaitTimeMessage(minutes, seconds);

                var response = new RateLimitResponse
                {
                    Success = false,
                    Error = "TooManyRequests",
                    Message = $"Rate limit exceeded. {waitTimeMessage}",
                    Details = new RateLimitDetails
                    {
                        Policy = policyName,
                        ClientIdentifier = MaskClientId(clientId),
                        RetryAfter = new RetryAfterInfo
                        {
                            TotalSeconds = totalSeconds,
                            Minutes = minutes,
                            Seconds = seconds,
                            FormattedTime = FormatTimeRemaining(minutes, seconds),
                            RetryAt = retryAtUtc,
                            RetryAtLocal = retryAtUtc.ToString("yyyy-MM-dd HH:mm:ss") + " UTC"
                        },
                        Suggestions = GetRateLimitSuggestions(policyName)
                    }
                };

                await context.HttpContext.Response.WriteAsJsonAsync(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                }, token);
            };

            // Auth policy: 5 requests per 15 minutes (per IP + device)
            options.AddPolicy(RateLimitPolicies.Auth, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetClientIdentifier(context),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(15),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            // OTP policy: 3 requests per 5 minutes (per IP + device)
            options.AddPolicy(RateLimitPolicies.Otp, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetClientIdentifier(context),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 3,
                        Window = TimeSpan.FromMinutes(5),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            // General policy: 100 requests per minute (per IP + device)
            options.AddPolicy(RateLimitPolicies.General, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetClientIdentifier(context),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 2
                    }));

            // File upload policy: 10 requests per minute (per IP + device)
            options.AddPolicy(RateLimitPolicies.FileUpload, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetClientIdentifier(context),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            // Payment policy: 10 requests per minute (per IP + device)
            options.AddPolicy(RateLimitPolicies.Payment, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetClientIdentifier(context),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            // Search policy: 30 requests per minute (per IP + device)
            options.AddPolicy(RateLimitPolicies.Search, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetClientIdentifier(context),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 30,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 2
                    }));

            // Admin policy: 50 requests per minute (per IP + device)
            options.AddPolicy(RateLimitPolicies.Admin, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetClientIdentifier(context),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 50,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 2
                    }));

            // Global limiter as fallback (per IP + device)
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var clientId = GetClientIdentifier(context);
                
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: clientId,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 200,
                        Window = TimeSpan.FromMinutes(5),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 5
                    });
            });
        });

        return services;
    }

    /// <summary>
    /// Gets a unique client identifier combining IP address and device fingerprint
    /// </summary>
    private static string GetClientIdentifier(HttpContext context)
    {
        // Get client IP (supports reverse proxies)
        var clientIp = GetClientIpAddress(context);
        
        // Get device fingerprint from User-Agent
        var userAgent = context.Request.Headers.UserAgent.ToString();
        var deviceFingerprint = GetDeviceFingerprint(userAgent);
        
        // Combine for unique identifier
        return $"{clientIp}:{deviceFingerprint}";
    }

    /// <summary>
    /// Gets client IP address, considering reverse proxies (X-Forwarded-For)
    /// </summary>
    private static string GetClientIpAddress(HttpContext context)
    {
        // Check X-Forwarded-For header (for reverse proxies like nginx, cloudflare)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // Take the first IP (original client)
            var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (ips.Length > 0)
            {
                return ips[0].Trim();
            }
        }

        // Check X-Real-IP header
        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp.Trim();
        }

        // Fallback to connection remote IP
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    /// <summary>
    /// Creates a simple device fingerprint from User-Agent
    /// </summary>
    private static string GetDeviceFingerprint(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
            return "unknown";

        // Create a hash-like fingerprint from user agent
        var hash = userAgent.GetHashCode();
        return Math.Abs(hash).ToString("X8");
    }

    /// <summary>
    /// Masks the client identifier for privacy in responses
    /// </summary>
    private static string MaskClientId(string clientId)
    {
        if (string.IsNullOrEmpty(clientId) || clientId.Length < 8)
            return "***";

        var parts = clientId.Split(':');
        if (parts.Length >= 2)
        {
            var ip = parts[0];
            var ipParts = ip.Split('.');
            if (ipParts.Length == 4)
            {
                // Mask middle octets: 192.*.*.123
                return $"{ipParts[0]}.***.***{ipParts[3]}:{parts[1]}";
            }
        }

        // For IPv6 or other formats
        return $"{clientId[..4]}***{clientId[^4..]}";
    }

    /// <summary>
    /// Builds a human-readable wait time message
    /// </summary>
    private static string BuildWaitTimeMessage(int minutes, int seconds)
    {
        if (minutes > 0 && seconds > 0)
            return $"Please wait {minutes} minute{(minutes > 1 ? "s" : "")} and {seconds} second{(seconds > 1 ? "s" : "")} before trying again.";
        else if (minutes > 0)
            return $"Please wait {minutes} minute{(minutes > 1 ? "s" : "")} before trying again.";
        else
            return $"Please wait {seconds} second{(seconds > 1 ? "s" : "")} before trying again.";
    }

    /// <summary>
    /// Formats time remaining as MM:SS or just seconds
    /// </summary>
    private static string FormatTimeRemaining(int minutes, int seconds)
    {
        if (minutes > 0)
            return $"{minutes:D2}:{seconds:D2}";
        return $"00:{seconds:D2}";
    }

    /// <summary>
    /// Gets helpful suggestions based on rate limit policy
    /// </summary>
    private static string[] GetRateLimitSuggestions(string policyName)
    {
        return policyName switch
        {
            RateLimitPolicies.Auth => new[]
            {
                "Check your credentials carefully before retrying",
                "Use the password reset feature if you forgot your password",
                "Contact support if you're experiencing issues"
            },
            RateLimitPolicies.Otp => new[]
            {
                "Check your email/phone for the OTP code",
                "OTP codes are valid for 10 minutes",
                "Request a new OTP after the cooldown period"
            },
            RateLimitPolicies.FileUpload => new[]
            {
                "Consider uploading files in batches",
                "Compress large files before uploading",
                "Check file size limits in documentation"
            },
            RateLimitPolicies.Payment => new[]
            {
                "Ensure payment details are correct before retrying",
                "Check your payment method is valid",
                "Contact support for payment assistance"
            },
            _ => new[]
            {
                "Reduce request frequency",
                "Implement request caching on your side",
                "Contact support if you need higher limits"
            }
        };
    }
}

#region Response Models

/// <summary>
/// Rate limit error response
/// </summary>
public class RateLimitResponse
{
    public bool Success { get; set; }
    public string Error { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public RateLimitDetails? Details { get; set; }
}

/// <summary>
/// Detailed rate limit information
/// </summary>
public class RateLimitDetails
{
    public string Policy { get; set; } = string.Empty;
    public string ClientIdentifier { get; set; } = string.Empty;
    public RetryAfterInfo? RetryAfter { get; set; }
    public string[]? Suggestions { get; set; }
}

/// <summary>
/// Retry timing information
/// </summary>
public class RetryAfterInfo
{
    public int TotalSeconds { get; set; }
    public int Minutes { get; set; }
    public int Seconds { get; set; }
    public string FormattedTime { get; set; } = string.Empty;
    public DateTime RetryAt { get; set; }
    public string RetryAtLocal { get; set; } = string.Empty;
}

#endregion
