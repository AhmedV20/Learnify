using Asp.Versioning;
using AutoMapper;
using Learnify.Application.Common.Pagination;
using Learnify.Application.OrderDetails.Commands.CreateOrderDetail;
using Learnify.Application.OrderDetails.Commands.DeleteOrderDetail;
using Learnify.Application.OrderDetails.Commands.UpdateOrderDetail;
using Learnify.Application.OrderDetails.DTOs.Requests;
using Learnify.Application.OrderDetails.DTOs.Responses;
using Learnify.Application.OrderDetails.Queries.GetAllOrderDetails;
using Learnify.Application.OrderDetails.Queries.GetOrderDetailById;
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
    public class OrderDetailsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        public OrderDetailsController(IMediator mediator, IMapper mapper)
        {
            _mapper = mapper;
            _mediator = mediator;
        }

        // GET: api/orderDetails
        [HttpGet]
        public async Task<ActionResult<PagedResult<OrderDetailResponse>>> GetAll([FromQuery] GetAllOrderDetailsQuery query)
        {
            var (orderDetails, totalCount) = await _mediator.Send(query);
            var orderDetailResponses = _mapper.Map<List<OrderDetailResponse>>(orderDetails);

            return Ok(new PagedResult<OrderDetailResponse>(orderDetailResponses, totalCount, query.PageNumber, query.PageSize));
        }

        // GET: api/orderDetails/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDetailResponse>> GetById(int id)
        {
            var query = new GetOrderDetailByIdQuery(id);
            var orderDetail = await _mediator.Send(query);

            if (orderDetail == null)
                return NotFound();

            return Ok(_mapper.Map<OrderDetailResponse>(orderDetail));
        }


        // POST: api/orderDetails
        [HttpPost]
        public async Task<ActionResult<OrderDetailResponse>> Create(CreateOrderDetailRequest request)
        {
            var command = _mapper.Map<CreateOrderDetailCommand>(request);

            var orderDetail = await _mediator.Send(command);
            var orderDetailResponse = _mapper.Map<OrderDetailResponse>(orderDetail);

            return CreatedAtAction(nameof(GetById), new { id = orderDetailResponse.Id }, orderDetailResponse);
        }


        // PUT: api/orderDetails/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<OrderDetailResponse>> Update(int id, UpdateOrderDetailRequest request)
        {
            var command = _mapper.Map<UpdateOrderDetailCommand>((request, id));

            var orderDetail = await _mediator.Send(command);

            if (orderDetail == null)
                return NotFound();

            return Ok(_mapper.Map<OrderDetailResponse>(orderDetail));
        }

        //DELETE: api/orderDetails/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var command = new DeleteOrderDetailCommand(id);
            var result = await _mediator.Send(command);

            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
