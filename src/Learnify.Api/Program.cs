using System;
using System.Linq;
using System.Threading.Tasks;
using Learnify.Api.Extensions;
using Learnify.Api.Health.Checks;
using Learnify.Api.Logging;
using Learnify.Api.Middleware;
using Learnify.Api.RateLimiting;
using Learnify.Application;
using Learnify.Infrastructure;
using Learnify.Infrastructure.UpdateDatabaseIntializerEx;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Learnify.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Create bootstrap logger for startup/shutdown logging
        SerilogConfiguration.CreateBootstrapLogger();

        try
        {
            Log.Information("Starting Learnify API");

            var builder = WebApplication.CreateBuilder(args);

            // Add Serilog logging (replaces default logging)
            builder.AddSerilogLogging();

            builder.Services.AddControllers(options =>
            {
                // Register API response wrapper filter globally
                // options.Filters.Add<Learnify.Api.Filters.ApiResponseActionFilter>();
            });
            builder.Services.AddEndpointsApiExplorer();

            // API Versioning (api/v1, api/v2)
            builder.Services.AddApiVersioningConfiguration();

            // Swagger/OpenAPI Documentation
            builder.Services.AddSwaggerConfiguration();

            // Rate Limiting
            builder.Services.AddRateLimitingPolicies();

            // CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // Application & Infrastructure layers
            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddHttpClient();

            // Hangfire background job processing
            builder.Services.AddHangfireServices(builder.Configuration);

            // Health Checks
            builder.Services.AddHealthChecks()
                .AddSqlServer(
                    connectionString: builder.Configuration.GetConnectionString("DefaultConnection")!,
                    name: "database",
                    failureStatus: HealthStatus.Unhealthy,
                    timeout: TimeSpan.FromSeconds(5),
                    tags: new[] { "db", "sql", "critical" })
                .AddCheck<StripeHealthCheck>(
                    "stripe",
                    failureStatus: HealthStatus.Degraded,
                    timeout: TimeSpan.FromSeconds(5),
                    tags: new[] { "external", "payments" })
                .AddCheck<CloudinaryHealthCheck>(
                    "cloudinary",
                    failureStatus: HealthStatus.Degraded,
                    timeout: TimeSpan.FromSeconds(5),
                    tags: new[] { "external", "storage" })
                .AddCheck<EmailHealthCheck>(
                    "email",
                    failureStatus: HealthStatus.Degraded,
                    timeout: TimeSpan.FromSeconds(5),
                    tags: new[] { "external", "email" });

                // Redis health check (commented out - uncomment if using Redis)
                // .AddRedis(
                //     redisConnectionString: builder.Configuration.GetConnectionString("Redis")!,
                //     name: "redis",
                //     failureStatus: HealthStatus.Degraded,
                //     timeout: TimeSpan.FromSeconds(3),
                //     tags: new[] { "cache", "redis" });

            // Health Check UI - Visual Dashboard
            builder.Services
                .AddHealthChecksUI(settings =>
                {
                    settings.SetEvaluationTimeInSeconds(500); // Check every 10 seconds
                    settings.MaximumHistoryEntriesPerEndpoint(50); // Keep last 50 checks
                    settings.AddHealthCheckEndpoint("Learnify API", "/health-api");
                })
                .AddInMemoryStorage();

            var app = builder.Build();

            // Add Serilog request logging middleware early in pipeline
            // This adds correlation IDs and request/response logging
            app.UseSerilogRequestLogging();

            if (app.Environment.IsDevelopment())
            {
                // API DOCUMENTATION            
                // OpenAPI spec generation (required by both Swagger UI and Scalar)
                app.UseSwagger();

                // Option 1: Scalar API Reference (Modern, feature-rich UI)
                app.UseScalarConfiguration();

                // Option 2: Swagger UI (Legacy - commented out for future use)
                // app.UseSwaggerConfiguration();

                // Auto-run database migrations
                await app.UseDatabaseMigrations();
            }

            app.UseStaticFiles();
            app.UseMiddleware<ExceptionHandlerMiddleware>();
            app.UseCors("AllowAllOrigins");
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();

            // Hangfire Dashboard - accessible at /hangfire (Admin only in production)
            app.UseHangfireDashboard();

            // Map Health Check endpoints
            app.MapHealthChecks("/health-api", new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var result = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        status = report.Status.ToString(),
                        totalDuration = $"{report.TotalDuration.TotalMilliseconds:F0}ms",
                        entries = report.Entries.Select(e => new
                        {
                            name = e.Key,
                            status = e.Value.Status.ToString(),
                            description = e.Value.Description,
                            duration = $"{e.Value.Duration.TotalMilliseconds:F0}ms",
                            tags = e.Value.Tags,
                            data = e.Value.Data.Any() ? e.Value.Data : null,
                            exception = e.Value.Exception?.Message
                        })
                    }, new System.Text.Json.JsonSerializerOptions 
                    { 
                        WriteIndented = true,
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                    });
                    await context.Response.WriteAsync(result);
                }
            });

            // Health Check UI Dashboard
            app.MapHealthChecksUI(options =>
            {
                options.UIPath = "/health-ui"; // Access at: http://localhost:5279/health-ui
                options.ApiPath = "/health-ui-api";
            });

            app.MapControllers();

            Log.Information("Learnify API started successfully");
            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.Information("Learnify API shutting down");
            SerilogConfiguration.CloseAndFlush();
        }
    }
}
