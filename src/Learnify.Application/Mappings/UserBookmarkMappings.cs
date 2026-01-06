using AutoMapper;
using Learnify.Application.UserBookmarks.Commands.CreateUserBookmark;
using Learnify.Application.UserBookmarks.Commands.UpdateUserBookmark;
using Learnify.Application.UserBookmarks.DTOs.Requests;
using Learnify.Application.UserBookmarks.DTOs.Responses;
using Learnify.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Application.Mappings;
 
class UserBookmarkMappings : Profile
{
    public UserBookmarkMappings()
    {
        // Domain to Response DTO
        CreateMap<UserBookmark, UserBookmarkResponse>();

        //command to Domain
        CreateMap<CreateUserBookmarkCommand, UserBookmark>();
        CreateMap<UpdateUserBookmarkCommand, UserBookmark>();



        // Request DTO to Command
        CreateMap<CreateUserBookmarkRequest, CreateUserBookmarkCommand>();

        //For UpdateUserBookmarkCommand, we need to handle the Id parameter
        CreateMap<(UpdateUserBookmarkRequest Request, int Id), UpdateUserBookmarkCommand>()
            .ConstructUsing(src => new UpdateUserBookmarkCommand(src.Id, src.Request.UserId, src.Request.CourseId, src.Request.CreatedAt));
    }
}
