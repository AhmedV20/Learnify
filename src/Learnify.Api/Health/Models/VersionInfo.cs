using System;

namespace Learnify.Api.Health.Models;

public class VersionInfo
{
    public string Version { get; set; } = "1.0.0";
    public DateTime BuildDate { get; set; }
    public string Environment { get; set; } = string.Empty;
    public string DotNetVersion { get; set; } = string.Empty;
    public TimeSpan Uptime { get; set; }
}
