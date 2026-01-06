using AutoMapper;
using Learnify.Application.Invoices.Commands.UpdateInvoice;
using Learnify.Application.OrderDetails.Commands.CreateOrderDetail;
using Learnify.Application.OrderDetails.DTOs.Requests;
using Learnify.Application.OrderDetails.DTOs.Responses;
using Learnify.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Learnify.Application.OrderDetails.Commands.UpdateOrderDetail;

namespace Learnify.Application.Mappings;

class OrderDetailMappings : Profile
{
    public OrderDetailMappings()
    {
        // Domain to Response DTO
        CreateMap<OrderDetail, OrderDetailResponse>();

        //command to Domain
        CreateMap<CreateOrderDetailCommand, OrderDetail>();
        CreateMap<UpdateOrderDetailCommand, OrderDetail>();




        // Request DTO to Command
        CreateMap<CreateOrderDetailRequest, CreateOrderDetailCommand>();

        // For UpdateOrderDetailCommand, we need to handle the Id parameter
        CreateMap<(UpdateOrderDetailRequest Request, int Id), UpdateOrderDetailCommand>()
            .ConstructUsing(src => new UpdateOrderDetailCommand(src.Id, src.Request.PaymentId, src.Request.EnrollmentId, src.Request.CourseId, src.Request.PriceAtPurchase));
    }
}