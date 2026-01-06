using Learnify.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Application.AppliedCoupons.Queries.GetAppliedCouponById;
 
public record GetAppliedCouponByIdQuery(int Id) : IRequest<AppliedCoupon>;
