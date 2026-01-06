using Learnify.Application.Carts.DTOs.Response;
using Learnify.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Application.Carts.Queries
{
    public record GetCartByUserIdQuery(string UserId) : IRequest<CartResponse?>;

}
