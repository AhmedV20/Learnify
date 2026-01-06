using AutoMapper;
using Learnify.Application.Common.Exceptions;
using Learnify.Application.Common.Interfaces;
using Learnify.Application.Users.DTOs.Response;
using Learnify.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;

namespace Learnify.Application.Users.Queries.GetUserProfile
{
    public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IImageUrlService _imageUrlService;

        public GetUserProfileQueryHandler(
            UserManager<ApplicationUser> userManager, 
            IMapper mapper,
            IImageUrlService imageUrlService)
        {
            _userManager = userManager;
            _mapper = mapper;
            _imageUrlService = imageUrlService;
        }

        public async Task<UserProfileResponse> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }

            var response = _mapper.Map<UserProfileResponse>(user);
            
            // Convert relative image URL to full absolute URL
            response.ProfilePictureUrl = _imageUrlService.GetFullUrl(user.ProfileImageUrl);
            
            return response;
        }
    }
} 