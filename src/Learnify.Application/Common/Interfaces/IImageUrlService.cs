namespace Learnify.Application.Common.Interfaces
{
    /// <summary>
    /// Service to resolve image URLs to full absolute URLs.
    /// </summary>
    public interface IImageUrlService
    {
        /// <summary>
        /// Converts a relative image path to a full absolute URL.
        /// </summary>
        /// <param name="relativePath">The relative path (e.g., /images/profiles/abc.jpg)</param>
        /// <returns>Full URL or null if the input is null/empty</returns>
        string? GetFullUrl(string? relativePath);
    }
}
