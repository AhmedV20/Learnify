using System;
using System.Collections.Generic;

namespace Learnify.Api.Health.Models;

public class HealthResponse
{
    public string Status { get; set; } = "Healthy";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Version { get; set; } = "1.0.0";
    public string Environment { get; set; } = string.Empty;
    public string Uptime { get; set; } = string.Empty;
    public Dictionary<string, HealthCheckDetail> Checks { get; set; } = new();
}
