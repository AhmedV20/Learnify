using AutoMapper;
using Learnify.Application.Common.Interfaces;
using Learnify.Application.Invoices.Commands.CreateInvoice;
using Learnify.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Application.OrderDetails.Commands.CreateOrderDetail;

public class CreateOrderDetailCommandHandler : IRequestHandler<CreateOrderDetailCommand, OrderDetail>
{
    private readonly IOrderDetailRepository _orderDetailRepository;
    private readonly IMapper _mapper;


    public CreateOrderDetailCommandHandler(IOrderDetailRepository orderDetailRepository, IMapper mapper)
    {
        _orderDetailRepository = orderDetailRepository;
        _mapper = mapper;
    }

    public async Task<OrderDetail> Handle(CreateOrderDetailCommand request, CancellationToken cancellationToken)
    {
        // Create orderDetail
        var orderDetail = _mapper.Map<OrderDetail>(request);

        // Add it to the db
        await _orderDetailRepository.AddOrderDetailAsync(orderDetail);

        // Return orderDetail
        return orderDetail;
    }
}