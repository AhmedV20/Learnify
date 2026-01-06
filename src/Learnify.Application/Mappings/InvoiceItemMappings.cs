using AutoMapper;
using Learnify.Application.InvoiceItems.Commands.CreateInvoiceItem;
using Learnify.Application.InvoiceItems.Commands.UpdateInvoiceItem;
using Learnify.Application.InvoiceItems.DTOs.Requests;
using Learnify.Application.InvoiceItems.DTOs.Responses;
using Learnify.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Application.Mappings;
 
class InvoiceItemMappings : Profile
{
    public InvoiceItemMappings()
    {
        // Domain to Response DTO
        CreateMap<InvoiceItem, InvoiceItemResponse>();

        //command to Domain
        CreateMap<CreateInvoiceItemCommand, InvoiceItem>();
        CreateMap<UpdateInvoiceItemCommand, InvoiceItem>();

        // Request DTO to Command
        CreateMap<CreateInvoiceItemRequest, CreateInvoiceItemCommand>();

        //For UpdateInvoiceItemCommand, we need to handle the Id parameter
        CreateMap<(UpdateInvoiceItemRequest Request, int Id), UpdateInvoiceItemCommand>()
            .ConstructUsing(src => new UpdateInvoiceItemCommand(src.Id, src.Request.InvoiceId, src.Request.CourseId, src.Request.Description, src.Request.OriginalUnitPrice, src.Request.DiscountAppliedOnItem, src.Request.UnitPrice));
    }
}

