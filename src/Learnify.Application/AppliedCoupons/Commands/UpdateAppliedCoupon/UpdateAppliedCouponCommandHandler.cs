using AutoMapper;
using Learnify.Application.Categories.Commands.UpdateCategory;
using Learnify.Application.Common.Exceptions;
using Learnify.Application.Common.Interfaces;
using Learnify.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Application.AppliedCoupons.Commands.UpdateAppliedCoupon;

public class UpdateAppliedCouponCommandHandler : IRequestHandler<UpdateAppliedCouponCommand, AppliedCoupon>
{
    private readonly IAppliedCouponRepository _appliedCouponRepository;
    private readonly IMapper _mapper;


    public UpdateAppliedCouponCommandHandler(IAppliedCouponRepository appliedCouponRepository, IMapper mapper)
    {
        _appliedCouponRepository = appliedCouponRepository;
        _mapper = mapper;
    }

    public async Task<AppliedCoupon> Handle(UpdateAppliedCouponCommand request, CancellationToken cancellationToken)
    {
        var appliedCoupon = await _appliedCouponRepository.GetAppliedCouponByIdAsync(request.Id);

        if (appliedCoupon == null)
        {
            throw new NotFoundException(nameof(AppliedCoupon), request.Id);
        }

        appliedCoupon = _mapper.Map<AppliedCoupon>(request);

        await _appliedCouponRepository.UpdateAppliedCouponAsync(appliedCoupon);

        return appliedCoupon;
    }
}