using Hangfire.Dashboard;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Learnify.Infrastructure.BackgroundJobs.Security;

/// <summary>
/// Authorization filter for Hangfire Dashboard.
/// - Development: Allows all access
/// - Production: Requires Admin role + logs access attempts
/// </summary>
public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        var logger = httpContext.RequestServices
            .GetRequiredService<ILogger<HangfireDashboardAuthorizationFilter>>();

        var env = httpContext.RequestServices
            .GetRequiredService<IWebHostEnvironment>();

        // Development: allow all access for debugging
        if (env.IsDevelopment())
        {
            logger.LogDebug("Hangfire dashboard accessed in Development mode");
            return true;
        }

        // Production: Admin role required
        var isAuthenticated = httpContext.User.Identity?.IsAuthenticated == true;
        var isAdmin = httpContext.User.IsInRole("Admin");
        var isAuthorized = isAuthenticated && isAdmin;

        if (isAuthorized)
        {
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userEmail = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;

            logger.LogInformation(
                "Hangfire dashboard accessed by Admin | UserId: {UserId} | Email: {Email} | IP: {IpAddress}",
                userId,
                MaskEmail(userEmail),
                httpContext.Connection.RemoteIpAddress?.ToString());
        }
        else
        {
            logger.LogWarning(
                "Unauthorized Hangfire dashboard access attempt | IsAuthenticated: {IsAuthenticated} | IsAdmin: {IsAdmin} | IP: {IpAddress}",
                isAuthenticated,
                isAdmin,
                httpContext.Connection.RemoteIpAddress?.ToString());
        }

        return isAuthorized;
    }

    private static string MaskEmail(string? email)
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
}
