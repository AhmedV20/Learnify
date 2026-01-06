using Learnify.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Application.UserBookmarks.Commands.CreateUserBookmark;

public record CreateUserBookmarkCommand(string UserId, int CourseId, DateTime CreatedAt) : IRequest<UserBookmark>;
