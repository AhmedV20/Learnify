using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CloudinaryDotNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Learnify.Api.Health.Checks;

public class CloudinaryHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public CloudinaryHealthCheck(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cloudName = _configuration["Cloudinary:CloudName"];
            var apiKey = _configuration["Cloudinary:ApiKey"];
            var apiSecret = _configuration["Cloudinary:ApiSecret"];

            if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
            {
                return HealthCheckResult.Degraded(
                    "Cloudinary credentials not configured",
                    data: new Dictionary<string, object> { ["configured"] = false });
            }

            var account = new Account(cloudName, apiKey, apiSecret);
            var cloudinary = new Cloudinary(account);
            
            // Simple ping to check if credentials are valid
            var result = await cloudinary.ListResourcesAsync(cancellationToken: cancellationToken);

            if (result.Error != null)
            {
                 return HealthCheckResult.Degraded(
                    $"Cloudinary API error: {result.Error.Message}");
            }

            return HealthCheckResult.Healthy(
                "Cloudinary API is accessible",
                data: new Dictionary<string, object>
                {
                    ["cloudName"] = cloudName
                });
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Failed to connect to Cloudinary",
                ex);
        }
    }
}
