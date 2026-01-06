using Asp.Versioning;
using Learnify.Api.Health.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace Learnify.Api.Health;

/// <summary>
/// Health check endpoints for monitoring API and dependencies
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;
    private readonly IWebHostEnvironment _env;
    private static readonly DateTime _startTime = DateTime.UtcNow;

    public HealthController(
        HealthCheckService healthCheckService,
        IWebHostEnvironment env)
    {
        _healthCheckService = healthCheckService;
        _env = env;
    }

    /// <summary>
    /// Get comprehensive health status of API and all dependencies
    /// </summary>
    /// <returns>Health status with dependency checks and response times</returns>
    /// <response code="200">API and all dependencies are healthy</response>
    /// <response code="503">One or more dependencies are unhealthy</response>
    [HttpGet("health")]
    [ProducesResponseType(typeof(HealthResponse), 200)]
    [ProducesResponseType(typeof(HealthResponse), 503)]
    public async Task<IActionResult> GetHealth()
    {
        var healthReport = await _healthCheckService.CheckHealthAsync();
        
        var response = new HealthResponse
        {
            Status = healthReport.Status.ToString(),
            Timestamp = DateTime.UtcNow,
            Version = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "1.0.0",
            Environment = _env.EnvironmentName,
            Uptime = FormatUptime(DateTime.UtcNow - _startTime),
            Checks = healthReport.Entries.ToDictionary(
                entry => entry.Key,
                entry => new HealthCheckDetail
                {
                    Status = entry.Value.Status.ToString(),
                    ResponseTime = $"{entry.Value.Duration.TotalMilliseconds:F0}ms",
                    Details = entry.Value.Data.Any() 
                        ? entry.Value.Data.ToDictionary(d => d.Key, d => d.Value)
                        : null
                }
            )
        };

        var statusCode = healthReport.Status == HealthStatus.Healthy ? 200 : 503;
        return StatusCode(statusCode, response);
    }

    /// <summary>
    /// Get API version and build information
    /// </summary>
    /// <returns>Version details including environment and uptime</returns>
    /// <response code="200">Version information retrieved successfully</response>
    [HttpGet("version")]
    [ProducesResponseType(typeof(VersionInfo), 200)]
    public IActionResult GetVersion()
    {
        return Ok(new VersionInfo
        {
            Version = "1.0.0",
            BuildDate = DateTime.UtcNow,
            Environment = _env.EnvironmentName,
            DotNetVersion = Environment.Version.ToString(),
            Uptime = DateTime.UtcNow - _startTime
        });
    }

    private static string FormatUptime(TimeSpan uptime)
    {
        if (uptime.TotalDays >= 1)
            return $"{(int)uptime.TotalDays}d {uptime.Hours}h {uptime.Minutes}m";
        if (uptime.TotalHours >= 1)
            return $"{(int)uptime.TotalHours}h {uptime.Minutes}m";
        return $"{(int)uptime.TotalMinutes}m {uptime.Seconds}s";
    }
}
