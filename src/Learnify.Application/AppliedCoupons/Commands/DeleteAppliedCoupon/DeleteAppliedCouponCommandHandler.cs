using AutoMapper;
using Learnify.Application.Common.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Application.AppliedCoupons.Commands.DeleteAppliedCoupon;
 
public class DeleteAppliedCouponCommandHandler : IRequestHandler<DeleteAppliedCouponCommand, bool>
{
    private readonly IAppliedCouponRepository _appliedCouponRepository;

    public DeleteAppliedCouponCommandHandler(IAppliedCouponRepository appliedCouponRepository, IMapper mapper)
    {
        _appliedCouponRepository = appliedCouponRepository;
    }

    public async Task<bool> Handle(DeleteAppliedCouponCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _appliedCouponRepository.DeleteAppliedCouponAsync(request.Id);
            return true;
        }
        catch
        {
            return false;
        }
    }
}