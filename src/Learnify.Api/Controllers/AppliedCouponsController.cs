using Asp.Versioning;
using AutoMapper;
using Learnify.Application.AppliedCoupons.Commands.CreateAppliedCoupon;
using Learnify.Application.AppliedCoupons.Commands.DeleteAppliedCoupon;
using Learnify.Application.AppliedCoupons.Commands.UpdateAppliedCoupon;
using Learnify.Application.AppliedCoupons.DTOs.Requests;
using Learnify.Application.AppliedCoupons.DTOs.Responses;
using Learnify.Application.AppliedCoupons.Queries.GetAllAppliedCoupons;
using Learnify.Application.AppliedCoupons.Queries.GetAppliedCouponById;
using Learnify.Application.Common.Pagination;

using Learnify.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Learnify.Api.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class AppliedCouponsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        public AppliedCouponsController(IMediator mediator, IMapper mapper)
        {
            _mapper = mapper;
            _mediator = mediator;
        }

        // GET: api/appliedCoupons
        [HttpGet]
        public async Task<ActionResult<PagedResult<AppliedCouponResponse>>> GetAll([FromQuery] GetAllAppliedCouponsQuery query)
        {
            var (appliedCoupons, totalCount) = await _mediator.Send(query);
            var appliedCouponResponses = _mapper.Map<List<AppliedCouponResponse>>(appliedCoupons);

            return Ok(new PagedResult<AppliedCouponResponse>(appliedCouponResponses, totalCount, query.PageNumber, query.PageSize));
        }

        // GET: api/appliedCoupons/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<AppliedCouponResponse>> GetById(int id)
        {
            var query = new GetAppliedCouponByIdQuery(id);
            var appliedCoupon = await _mediator.Send(query);

            if (appliedCoupon == null)
                return NotFound();

            return Ok(_mapper.Map<AppliedCouponResponse>(appliedCoupon));
        }


        // POST: api/appliedCoupons
        [HttpPost]
        public async Task<ActionResult<AppliedCouponResponse>> Create(CreateAppliedCouponRequest request)
        {
            var command = _mapper.Map<CreateAppliedCouponCommand>(request);

            var appliedCoupon = await _mediator.Send(command);
            var appliedCouponResponse = _mapper.Map<AppliedCouponResponse>(appliedCoupon);

            return CreatedAtAction(nameof(GetById), new { id = appliedCouponResponse.Id }, appliedCouponResponse);
        }


        // PUT: api/appliedCoupons/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<AppliedCouponResponse>> Update(int id, UpdateAppliedCouponRequest request)
        {
            var command = _mapper.Map<UpdateAppliedCouponCommand>((request, id));

            var appliedCoupon = await _mediator.Send(command);

            if (appliedCoupon == null)
                return NotFound();

            return Ok(_mapper.Map<AppliedCouponResponse>(appliedCoupon));
        }

        //DELETE: api/appliedCoupons/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var command = new DeleteAppliedCouponCommand(id);
            var result = await _mediator.Send(command);

            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
