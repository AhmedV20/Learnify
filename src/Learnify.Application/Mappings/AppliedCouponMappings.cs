using AutoMapper;
using Learnify.Application.AppliedCoupons.Commands.CreateAppliedCoupon;
using Learnify.Application.AppliedCoupons.Commands.UpdateAppliedCoupon;
using Learnify.Application.AppliedCoupons.DTOs.Requests;
using Learnify.Application.AppliedCoupons.DTOs.Responses;
using Learnify.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Application.Mappings;

class AppliedCouponMappings : Profile
{
    public AppliedCouponMappings()
    {
        // Domain to Response DTO
        CreateMap<AppliedCoupon, AppliedCouponResponse>();

        //command to Domain
        CreateMap<CreateAppliedCouponCommand, AppliedCoupon>();
        CreateMap<UpdateAppliedCouponCommand, AppliedCoupon>();


        // Request DTO to Command
        CreateMap<CreateAppliedCouponRequest, CreateAppliedCouponCommand>();

        // For UpdateInvoiceCommand, we need to handle the Id parameter
        CreateMap<(UpdateAppliedCouponRequest Request, int Id), UpdateAppliedCouponCommand>()
            .ConstructUsing(src => new UpdateAppliedCouponCommand(src.Id, src.Request.CouponId, src.Request.UserId, src.Request.PaymentId, src.Request.DiscountAmountApplied, src.Request.AppliedAt));

    }
}