using Learnify.Application.Common.Exceptions;
using Learnify.Application.Common.Interfaces;
using Learnify.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Application.OrderDetails.Queries.GetOrderDetailById;

public class GetOrderDetailByIdQueryHandler : IRequestHandler<GetOrderDetailByIdQuery, OrderDetail>
{
    private readonly IOrderDetailRepository _orderDetailRepository;

    public GetOrderDetailByIdQueryHandler(IOrderDetailRepository orderDetailRepository)
    {
        _orderDetailRepository = orderDetailRepository;
    }

    public async Task<OrderDetail> Handle(GetOrderDetailByIdQuery request, CancellationToken cancellationToken)
    {
        var orderDetail = await _orderDetailRepository.GetOrderDetailByIdAsync(request.Id);

        if (orderDetail == null)
        {
            throw new NotFoundException(nameof(OrderDetail), request.Id);
        }

        return orderDetail;
    }
}