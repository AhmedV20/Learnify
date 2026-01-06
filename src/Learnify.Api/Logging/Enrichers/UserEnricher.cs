using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;
using System.Linq;
using System.Security.Claims;

namespace Learnify.Api.Logging.Enrichers;

/// <summary>
/// Serilog enricher that adds comprehensive user and request context to all log events.
/// Extracts UserId, masked email, roles, authentication status, client IP, and user agent.
/// </summary>
public class UserEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return;

        var user = httpContext.User;

        // Authentication status
        var isAuthenticated = user?.Identity?.IsAuthenticated ?? false;
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("IsAuthenticated", isAuthenticated));

        if (isAuthenticated)
        {
            // User ID
            var userId = user!.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserId", userId));

            // Masked email
            var email = user.FindFirst(ClaimTypes.Email)?.Value ?? "unknown";
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserEmail", MaskEmail(email)));

            // All user roles (not just the first one)
            var roles = user.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();
            
            if (roles.Any())
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserRoles", roles));
            }
            else
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserRoles", new[] { "none" }));
            }
        }
        else
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserId", "anonymous"));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserEmail", "anonymous"));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserRoles", new[] { "anonymous" }));
        }

        // Client IP (with X-Forwarded-For support for proxies/load balancers)
        var clientIp = GetClientIp(httpContext);
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ClientIP", clientIp));

        // User Agent (truncated for readability)
        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
        if (!string.IsNullOrEmpty(userAgent))
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserAgent", TruncateUserAgent(userAgent)));
        }
    }

    /// <summary>
    /// Masks email addresses for privacy in logs.
    /// Example: "john.doe@example.com" -> "j***e@example.com"
    /// </summary>
    private static string MaskEmail(string email)
    {
        if (string.IsNullOrEmpty(email) || !email.Contains('@'))
            return "***";

        var parts = email.Split('@');
        var localPart = parts[0];
        var domain = parts[1];

        if (localPart.Length <= 2)
            return $"**@{domain}";

        return $"{localPart[0]}***{localPart[^1]}@{domain}";
    }

    /// <summary>
    /// Gets client IP, checking X-Forwarded-For header first for proxy support.
    /// </summary>
    private static string GetClientIp(HttpContext context)
    {
        // Check X-Forwarded-For header (set by proxies/load balancers)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // Take the first IP (original client) from comma-separated list
            return forwardedFor.Split(',')[0].Trim();
        }

        // Fallback to direct connection IP
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    /// <summary>
    /// Truncates user agent string to prevent overly long log entries.
    /// </summary>
    private static string TruncateUserAgent(string userAgent)
    {
        const int maxLength = 100;
        return userAgent.Length > maxLength 
            ? userAgent[..maxLength] + "..." 
            : userAgent;
    }
}

