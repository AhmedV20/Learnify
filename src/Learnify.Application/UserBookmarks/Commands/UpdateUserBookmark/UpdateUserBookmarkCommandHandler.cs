using AutoMapper;
using Learnify.Application.Common.Exceptions;
using Learnify.Application.Common.Interfaces;
using Learnify.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Application.UserBookmarks.Commands.UpdateUserBookmark;

public class UpdateUserBookmarkCommandHandler : IRequestHandler<UpdateUserBookmarkCommand, UserBookmark>
{
    private readonly IUserBookmarkRepository _userBookmarkRepository;
    private readonly IMapper _mapper;
    public UpdateUserBookmarkCommandHandler(IUserBookmarkRepository userBookmarkRepository, IMapper mapper)
    {
        _userBookmarkRepository = userBookmarkRepository;
        _mapper = mapper;
    }

    public async Task<UserBookmark> Handle(UpdateUserBookmarkCommand request, CancellationToken cancellationToken)
    {
        var userBookmark = await _userBookmarkRepository.GetUserBookmarkByIdAsync(request.Id);

        if (userBookmark == null)
        {
            throw new NotFoundException(nameof(userBookmark), request.Id);
        }

        userBookmark = _mapper.Map<UserBookmark>(request);

        await _userBookmarkRepository.UpdateUserBookmarkAsync(userBookmark);

        return userBookmark;
    }
}

