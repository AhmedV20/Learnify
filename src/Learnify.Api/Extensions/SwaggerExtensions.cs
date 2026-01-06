using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.IO;
using System.Reflection;

namespace Learnify.Api.Extensions;

/// <summary>
/// Extension methods for configuring Swagger/OpenAPI documentation
/// </summary>
public static class SwaggerExtensions
{
    /// <summary>
    /// Adds Swagger generation services with professional API documentation
    /// </summary>
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            // API Version 1 Documentation
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Learnify API",
                Version = "v1",
                Description = GetApiDescription("v1"),
                Contact = new OpenApiContact
                {
                    Name = "Learnify Support",
                    Email = "support@learnify.com",
                    Url = new Uri("https://learnify.com/support")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                },
                TermsOfService = new Uri("https://learnify.com/terms")
            });

            // API Version 2 Documentation
            options.SwaggerDoc("v2", new OpenApiInfo
            {
                Title = "Learnify API",
                Version = "v2",
                Description = GetApiDescription("v2"),
                Contact = new OpenApiContact
                {
                    Name = "Learnify Support",
                    Email = "support@learnify.com",
                    Url = new Uri("https://learnify.com/support")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

            // Include XML comments for rich documentation
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            // JWT Bearer Authentication
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Enter your JWT token. Example: `eyJhbGciOiJIUzI1NiIs...`",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Enable annotations for better documentation
            options.EnableAnnotations();
        });

        return services;
    }

    /// <summary>
    /// Configures Swagger UI middleware with enhanced features
    /// </summary>
    public static WebApplication UseSwaggerConfiguration(this WebApplication app)
    {
        app.UseSwagger();

        app.UseSwaggerUI(options =>
        {
            // API Version Endpoints
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Learnify API v1");
            options.SwaggerEndpoint("/swagger/v2/swagger.json", "Learnify API v2");

            // UI Customization
            options.RoutePrefix = string.Empty;
            options.DocumentTitle = "Learnify API Documentation";

            // Enhanced Features
            options.EnableDeepLinking();           // Shareable URLs to specific endpoints
            options.EnablePersistAuthorization();  // Keep JWT token after page refresh
            options.DisplayRequestDuration();      // Show request execution time
            options.EnableTryItOutByDefault();     // Enable "Try it out" by default

            // Expand operations for better visibility
            options.DocExpansion(DocExpansion.List);

            // Custom CSS for dark theme
            //options.InjectStylesheet("/swagger-ui/swagger-custom.css");
        });

        return app;
    }

    private static string GetApiDescription(string version)
    {
        if (version == "v1")
        {
            return @"
## 🎓 Learnify E-Learning Platform API

A comprehensive e-learning platform API built with **Clean Architecture** principles.

### Features
- 📚 **Course Management** - Create, update, and manage courses
- 👥 **User Authentication** - JWT-based secure authentication with OTP verification
- 💳 **Payment Processing** - Multiple payment methods including manual payments
- 📊 **Analytics** - Track learning progress and instructor performance
- 🤖 **AI Chat** - Integrated AI assistant for learning support
- ⭐ **Ratings & Reviews** - Course rating and feedback system

### Authentication
All protected endpoints require a valid JWT token in the Authorization header:
```
Authorization: Bearer {your_token}
```
";
        }

        return @"
## 🎓 Learnify E-Learning Platform API v2

**Version 2** includes enhanced features and improved response formats.

### What's New in v2
- 🚀 Improved performance
- 📱 Better mobile support
- 🔄 Enhanced pagination
- 📈 More detailed analytics

> **Note:** v1 endpoints remain available for backward compatibility.
";
    }
}
