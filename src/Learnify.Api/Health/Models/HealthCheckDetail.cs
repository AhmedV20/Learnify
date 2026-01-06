using System.Collections.Generic;

namespace Learnify.Api.Health.Models;

public class HealthCheckDetail
{
    public string Status { get; set; } = string.Empty;
    public string ResponseTime { get; set; } = string.Empty;
    public Dictionary<string, object>? Details { get; set; }
}
