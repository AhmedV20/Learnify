using Learnify.Api.Logging.Enrichers;
using Learnify.Api.Logging.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Learnify.Api.Logging;

/// <summary>
/// Extension methods for configuring Serilog logging throughout the application.
/// </summary>
public static class SerilogConfiguration
{
    /// <summary>
    /// Adds Serilog logging to the application with enrichers and configuration.
    /// Also registers performance logging options.
    /// </summary>
    public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder)
    {
        // Register performance logging options from configuration
        builder.Services.Configure<PerformanceLoggingOptions>(
            builder.Configuration.GetSection(PerformanceLoggingOptions.SectionName));

        builder.Host.UseSerilog((context, services, loggerConfiguration) =>
        {
            // Read configuration from appsettings.json / appsettings.{Environment}.json
            loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services);

            // Add custom enricher for user context
            var httpContextAccessor = services.GetService<IHttpContextAccessor>();
            if (httpContextAccessor != null)
            {
                loggerConfiguration.Enrich.With(new UserEnricher(httpContextAccessor));
            }
        });

        return builder;
    }

    /// <summary>
    /// Adds Serilog request logging middleware to the HTTP pipeline.
    /// Should be called early in the middleware pipeline.
    /// Includes: CorrelationId -> Performance -> Request -> Response logging.
    /// </summary>
    public static WebApplication UseSerilogRequestLogging(this WebApplication app)
    {
        // 1. Correlation ID middleware first - ensures all logs have correlation ID
        app.UseMiddleware<CorrelationIdMiddleware>();

        // 2. Performance logging - tracks request duration and categorizes as NORMAL/SLOW/VERY_SLOW
        app.UseMiddleware<PerformanceLoggingMiddleware>();

        // 3. Request logging - logs request details with masked sensitive data
        app.UseMiddleware<RequestLoggingMiddleware>();

        // 4. Response logging - logs response bodies for debugging (especially errors)
        app.UseMiddleware<ResponseLoggingMiddleware>();

        return app;
    }

    /// <summary>
    /// Creates a bootstrap logger for startup/shutdown logging.
    /// This is used before the full logger is configured.
    /// </summary>
    public static void CreateBootstrapLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger();
    }

    /// <summary>
    /// Safely closes and flushes the logger.
    /// Should be called in the finally block of Program.cs.
    /// </summary>
    public static void CloseAndFlush()
    {
        Log.CloseAndFlush();
    }
}

