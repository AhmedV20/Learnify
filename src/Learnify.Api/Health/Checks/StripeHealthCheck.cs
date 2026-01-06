using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Stripe;

namespace Learnify.Api.Health.Checks;

public class StripeHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public StripeHealthCheck(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var secretKey = _configuration["Stripe:SecretKey"];
            if (string.IsNullOrEmpty(secretKey) || secretKey.StartsWith("sk_test_YOUR"))
            {
                return HealthCheckResult.Degraded(
                    "Stripe API key not configured",
                    data: new Dictionary<string, object> { ["configured"] = false });
            }

            StripeConfiguration.ApiKey = secretKey;
            var service = new BalanceService();
            var balance = await service.GetAsync(cancellationToken: cancellationToken);

            return HealthCheckResult.Healthy(
                "Stripe API is accessible",
                data: new Dictionary<string, object>
                {
                    ["available"] = balance.Available.Sum(b => b.Amount),
                    ["currency"] = balance.Available.FirstOrDefault()?.Currency ?? "usd"
                });
        }
        catch (StripeException ex)
        {
            return HealthCheckResult.Degraded(
                $"Stripe API error: {ex.StripeError?.Message ?? ex.Message}",
                ex);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Failed to connect to Stripe",
                ex);
        }
    }
}
