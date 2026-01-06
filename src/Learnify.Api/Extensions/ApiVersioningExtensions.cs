using Asp.Versioning;
using Microsoft.Extensions.DependencyInjection;

namespace Learnify.Api.Extensions;

/// <summary>
/// Extension methods for configuring API versioning
/// </summary>
public static class ApiVersioningExtensions
{
    /// <summary>
    /// Adds API versioning services to the application
    /// </summary>
    public static IServiceCollection AddApiVersioningConfiguration(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            // Default to v1 when no version is specified
            options.DefaultApiVersion = new ApiVersion(1, 0);
            
            // Assume default version when unspecified
            options.AssumeDefaultVersionWhenUnspecified = true;
            
            // Report supported versions in response headers
            options.ReportApiVersions = true;
            
            // Read version from URL segment (api/v1/...)
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddApiExplorer(options =>
        {
            // Format: 'v'major[.minor][-status]
            options.GroupNameFormat = "'v'VVV";
            
            // Substitute version in URL
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
}
