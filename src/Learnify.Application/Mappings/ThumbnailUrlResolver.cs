using AutoMapper;
using Learnify.Application.Common.Interfaces;
using Learnify.Application.Courses.DTOs;
using Learnify.Application.Lectures.DTOs;
using Learnify.Domain.Entities;

namespace Learnify.Application.Mappings
{
    /// <summary>
    /// AutoMapper value resolver that converts relative image URLs to full absolute URLs.
    /// </summary>
    public class ThumbnailUrlResolver : IValueResolver<Course, CourseResponse, string?>
    {
        private readonly IImageUrlService _imageUrlService;

        public ThumbnailUrlResolver(IImageUrlService imageUrlService)
        {
            _imageUrlService = imageUrlService;
        }

        public string? Resolve(Course source, CourseResponse destination, string? destMember, ResolutionContext context)
        {
            return _imageUrlService.GetFullUrl(source.ThumbnailImageUrl);
        }
    }

    /// <summary>
    /// AutoMapper value resolver for CourseWithContentResponse.
    /// </summary>
    public class ThumbnailUrlResolverWithContent : IValueResolver<Course, CourseWithContentResponse, string?>
    {
        private readonly IImageUrlService _imageUrlService;

        public ThumbnailUrlResolverWithContent(IImageUrlService imageUrlService)
        {
            _imageUrlService = imageUrlService;
        }

        public string? Resolve(Course source, CourseWithContentResponse destination, string? destMember, ResolutionContext context)
        {
            return _imageUrlService.GetFullUrl(source.ThumbnailImageUrl);
        }
    }

    /// <summary>
    /// AutoMapper value resolver for lecture video URLs.
    /// </summary>
    public class VideoUrlResolver : IValueResolver<Lecture, LectureResponse, string>
    {
        private readonly IImageUrlService _imageUrlService;

        public VideoUrlResolver(IImageUrlService imageUrlService)
        {
            _imageUrlService = imageUrlService;
        }

        public string Resolve(Lecture source, LectureResponse destination, string destMember, ResolutionContext context)
        {
            return _imageUrlService.GetFullUrl(source.VideoUrl) ?? string.Empty;
        }
    }
}
