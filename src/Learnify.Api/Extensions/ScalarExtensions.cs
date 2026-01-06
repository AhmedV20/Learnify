using Microsoft.AspNetCore.Builder;
using Scalar.AspNetCore;

namespace Learnify.Api.Extensions;

/// <summary>
/// Extension methods for configuring Scalar API documentation
/// </summary>
public static class ScalarExtensions
{
    /// <summary>
    /// Configures Scalar API Reference middleware with enhanced features
    /// </summary>
    public static WebApplication UseScalarConfiguration(this WebApplication app)
    {
        // Scalar API Reference - Modern API documentation UI
        // Access at: /scalar/v1 or /scalar/v2
        app.MapScalarApiReference(options =>
        {
            // BASIC SETTINGS
            
            options
                // API Title displayed in the documentation header
                .WithTitle("Learnify API Documentation")
                
                // Dark/Light theme options: 
                // None, Alternate, Moon, Purple, Solarized, BluePlanet, Saturn, Kepler, Mars, DeepSpace, Default
                .WithTheme(ScalarTheme.Kepler)
                
                // Enable dark mode toggle (default: true)
                .WithDarkModeToggle(true)
                
                // Sidebar visibility (default: true)
                .WithSidebar(true)

                .WithLayout(ScalarLayout.Classic)

                // Set favicon for the documentation page
                .WithFavicon("/favicon.ico");

            // OPENAPI DOCUMENT CONFIGURATION
            
            options
                // OpenAPI document endpoints (Swagger JSON from Swashbuckle)
                // Pattern: {documentName} will be replaced with v1, v2, etc.
                .WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json");

            // JWT BEARER AUTHENTICATION
            
            options
                // Preferred authentication scheme (matches the scheme defined in Swagger)
                .WithPreferredScheme("Bearer")
                
                // HTTP Bearer authentication configuration
                .WithHttpBearerAuthentication(bearer =>
                {
                    // Default token value (empty - user will input their token)
                    bearer.Token = "";
                });

            // CODE GENERATION SETTINGS
            
            options
                // Default HTTP client shown in code examples
                // Targets: Shell, C, CSharp, Clojure, Go, Http, Java, JavaScript, Kotlin, 
                // Node, ObjC, OCaml, Php, PowerShell, Python, R, Ruby, Swift
                // 
                // Clients per target vary (e.g., JavaScript: Fetch, Axios, jQuery, XHR)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);

            // SEARCH CONFIGURATION
            
            options
                // Enable search functionality - Ctrl+K to open search
                .WithSearchHotKey("k");
        });

        return app;
    }
}
