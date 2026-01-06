using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Learnify.Api.Health.Checks;

public class EmailHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public EmailHealthCheck(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var smtpServer = _configuration["Email:SmtpServer"];
            var smtpPort = _configuration.GetValue<int>("Email:SmtpPort", 587);

            if (string.IsNullOrEmpty(smtpServer))
            {
                return HealthCheckResult.Degraded(
                    "SMTP server not configured",
                    data: new Dictionary<string, object> { ["configured"] = false });
            }

            using var client = new TcpClient();
            var connectTask = client.ConnectAsync(smtpServer, smtpPort);
            
            if (await Task.WhenAny(connectTask, Task.Delay(5000, cancellationToken)) != connectTask)
            {
                return HealthCheckResult.Degraded(
                    "SMTP connection timeout",
                    data: new Dictionary<string, object> { ["server"] = smtpServer, ["port"] = smtpPort });
            }

            await connectTask;

            return HealthCheckResult.Healthy(
                "SMTP server is accessible",
                data: new Dictionary<string, object> { ["server"] = smtpServer, ["port"] = smtpPort });
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Degraded(
                "Failed to connect to SMTP server",
                ex);
        }
    }
}