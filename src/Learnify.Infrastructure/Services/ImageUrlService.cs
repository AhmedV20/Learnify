using Learnify.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Learnify.Infrastructure.Services
{
    public class ImageUrlService : IImageUrlService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ImageUrlService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? GetFullUrl(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return null;
            }

            // If already a full URL, return as-is
            if (relativePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                relativePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return relativePath;
            }

            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null)
            {
                // Fallback if no HTTP context is available
                return relativePath;
            }

            // Build the base URL from the current request
            var baseUrl = $"{request.Scheme}://{request.Host}";

            // Ensure the path starts with /
            var path = relativePath.StartsWith('/') ? relativePath : $"/{relativePath}";

            return $"{baseUrl}{path}";
        }
    }
}
