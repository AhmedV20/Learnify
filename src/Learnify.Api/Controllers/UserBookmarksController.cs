using Asp.Versioning;
using AutoMapper;
using Learnify.Application.Common.Pagination;
using Learnify.Application.Invoices.Commands.CreateInvoice;
using Learnify.Application.Invoices.Commands.Delete_Invoice;
using Learnify.Application.Invoices.Commands.UpdateInvoice;
using Learnify.Application.Invoices.DTOs.Requests;
using Learnify.Application.Invoices.DTOs.Responses;
using Learnify.Application.Invoices.Queries.GetAllInvoices;
using Learnify.Application.Invoices.Queries.GetInvoiceById;
using Learnify.Application.UserBookmarks.Commands.CreateUserBookmark;
using Learnify.Application.UserBookmarks.Commands.DeleteUserBookmark;
using Learnify.Application.UserBookmarks.Commands.UpdateUserBookmark;
using Learnify.Application.UserBookmarks.DTOs.Requests;
using Learnify.Application.UserBookmarks.DTOs.Responses;
using Learnify.Application.UserBookmarks.Queries.GetAllUserBookmarks;
using Learnify.Application.UserBookmarks.Queries.GetUserBookmarkById;
using Learnify.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Learnify.Api.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
    [ApiVersion("1.0")]

public class UserBookmarksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    public UserBookmarksController(IMediator mediator, IMapper mapper)
    {
        _mapper = mapper;
        _mediator = mediator;
    }

    // GET: api/userBookmarks
    [HttpGet]
    public async Task<ActionResult<PagedResult<UserBookmarkResponse>>> GetAll([FromQuery] GetAllUserBookmarksQuery query)
    {
        var (userBookmarks, totalCount) = await _mediator.Send(query);
        var userBookmarksResponses = _mapper.Map<List<UserBookmarkResponse>>(userBookmarks);

        return Ok(new PagedResult<UserBookmarkResponse>(userBookmarksResponses, totalCount, query.PageNumber, query.PageSize));
    }

    // GET: api/userBookmark/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<UserBookmarkResponse>> GetById(int id)
    {
        var query = new GetUserBookmarkByIdQuery(id);
        var userBookmark = await _mediator.Send(query);

        if (userBookmark == null)
            return NotFound();

        return Ok(_mapper.Map<UserBookmarkResponse>(userBookmark));
    }


    // POST: api/userBookmark
    [HttpPost]
    public async Task<ActionResult<UserBookmarkResponse>> Create(CreateUserBookmarkRequest request)
    {
        var command = _mapper.Map<CreateUserBookmarkCommand>(request);

        var userBookmark = await _mediator.Send(command);
        var userBookmarkResponse = _mapper.Map<UserBookmarkResponse>(userBookmark);

        return CreatedAtAction(nameof(GetById), new { id = userBookmarkResponse.Id }, userBookmarkResponse);
    }


    // PUT: api/userBookmark/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<UserBookmarkResponse>> Update(int id, UpdateUserBookmarkRequest request)
    {
        var command = _mapper.Map<UpdateUserBookmarkCommand>((request, id));

        var userBookmark = await _mediator.Send(command);

        if (userBookmark == null)
            return NotFound();

        return Ok(_mapper.Map<UserBookmarkResponse>(userBookmark));
    }

    //DELETE: api/userBookmark/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var command = new DeleteUserBookmarkCommand(id);
        var result = await _mediator.Send(command);

        if (!result)
            return NotFound();

        return NoContent();
    }
}
