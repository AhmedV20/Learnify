using Learnify.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Application.AppliedCoupons.Commands.DeleteAppliedCoupon;

public record DeleteAppliedCouponCommand(int Id) : IRequest<bool>;

