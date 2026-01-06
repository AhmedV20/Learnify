using Asp.Versioning;
using Learnify.Application.ManualPayments;
using Learnify.Application.ManualPayments.DTOs;
using Learnify.Application.Common.Interfaces;
using Learnify.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Learnify.Api.Controllers;

/// <summary>
/// User endpoints for manual payments
/// </summary>
[ApiController]
    [ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/manual-payments")]
public class ManualPaymentsController : ControllerBase
{
    private readonly IManualPaymentRepository _repository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICartRepository _cartRepository;

    public ManualPaymentsController(
        IManualPaymentRepository repository,
        UserManager<ApplicationUser> userManager,
        ICartRepository cartRepository)
    {
        _repository = repository;
        _userManager = userManager;
        _cartRepository = cartRepository;
    }

    /// <summary>
    /// Check if manual payment is enabled
    /// </summary>
    [HttpGet("settings")]
    public async Task<ActionResult<ManualPaymentSettingsDto>> GetSettings()
    {
        var isEnabled = await _repository.IsManualPaymentEnabledAsync();
        return Ok(new ManualPaymentSettingsDto(isEnabled));
    }

    /// <summary>
    /// Get active payment methods (visible to users when enabled)
    /// </summary>
    [HttpGet("methods")]
    public async Task<ActionResult<List<ManualPaymentMethodDto>>> GetActiveMethods()
    {
        var isEnabled = await _repository.IsManualPaymentEnabledAsync();
        if (!isEnabled)
        {
            return Ok(new List<ManualPaymentMethodDto>());
        }

        var methods = await _repository.GetActiveMethodsAsync();
        var dtos = methods.Select(m => new ManualPaymentMethodDto(
            m.Id,
            m.Name,
            m.NameAr,
            m.Type,
            m.AccountIdentifier,
            m.AccountName,
            m.Instructions,
            m.InstructionsAr,
            m.IconUrl,
            m.DisplayOrder
        )).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// Submit a payment request with proof
    /// </summary>
    [HttpPost("submit")]
    [Authorize]
    public async Task<ActionResult<ManualPaymentRequestDto>> SubmitPayment([FromBody] SubmitPaymentRequest request)
    {
        var isEnabled = await _repository.IsManualPaymentEnabledAsync();
        if (!isEnabled)
        {
            return BadRequest("Manual payment is currently disabled");
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // Get user's cart
        var cart = await _cartRepository.GetCartAsync(userId);
        if (cart == null || !cart.Items.Any())
        {
            return BadRequest("Your cart is empty");
        }

        // Verify payment method exists
        var method = await _repository.GetMethodByIdAsync(request.PaymentMethodId);
        if (method == null || !method.IsActive)
        {
            return BadRequest("Invalid payment method");
        }

        // Calculate total
        var totalAmount = cart.Items.Sum(i => i.Price);

        // Create payment request
        var paymentRequest = new ManualPaymentRequest
        {
            UserId = userId,
            PaymentMethodId = request.PaymentMethodId,
            Amount = totalAmount,
            ProofImageUrl = request.ProofImageUrl,
            UserMessage = request.UserMessage,
            Status = "Pending"
        };

        // Add cart items
        foreach (var item in cart.Items)
        {
            paymentRequest.CartItems.Add(new ManualPaymentCartItem
            {
                CourseId = item.CourseId,
                Price = item.Price
            });
        }

        var created = await _repository.CreateRequestAsync(paymentRequest);

        // Clear the cart after submission
        await _cartRepository.DeleteCartAsync(userId);

        // Return the created request
        var dto = new ManualPaymentRequestDto(
            created.Id,
            created.PaymentMethodId,
            method.Name,
            created.Amount,
            created.ProofImageUrl,
            created.UserMessage,
            created.Status,
            created.AdminNote,
            created.CreatedAt,
            created.ReviewedAt,
            created.CartItems.Select(ci => new ManualPaymentCartItemDto(
                ci.CourseId,
                ci.Course?.Title ?? "",
                ci.Course?.ThumbnailImageUrl,
                ci.Price
            )).ToList()
        );

        return CreatedAtAction(nameof(GetMyRequest), new { id = created.Id }, dto);
    }

    /// <summary>
    /// Get user's payment requests
    /// </summary>
    [HttpGet("my-requests")]
    [Authorize]
    public async Task<ActionResult<List<ManualPaymentRequestDto>>> GetMyRequests()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var requests = await _repository.GetRequestsByUserIdAsync(userId);
        var dtos = requests.Select(r => new ManualPaymentRequestDto(
            r.Id,
            r.PaymentMethodId,
            r.PaymentMethod?.Name ?? "",
            r.Amount,
            r.ProofImageUrl,
            r.UserMessage,
            r.Status,
            r.AdminNote,
            r.CreatedAt,
            r.ReviewedAt,
            r.CartItems.Select(ci => new ManualPaymentCartItemDto(
                ci.CourseId,
                ci.Course?.Title ?? "",
                ci.Course?.ThumbnailImageUrl,
                ci.Price
            )).ToList()
        )).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// Get a specific payment request
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<ManualPaymentRequestDto>> GetMyRequest(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var request = await _repository.GetRequestByIdAsync(id);
        if (request == null || request.UserId != userId)
        {
            return NotFound();
        }

        var dto = new ManualPaymentRequestDto(
            request.Id,
            request.PaymentMethodId,
            request.PaymentMethod?.Name ?? "",
            request.Amount,
            request.ProofImageUrl,
            request.UserMessage,
            request.Status,
            request.AdminNote,
            request.CreatedAt,
            request.ReviewedAt,
            request.CartItems.Select(ci => new ManualPaymentCartItemDto(
                ci.CourseId,
                ci.Course?.Title ?? "",
                ci.Course?.ThumbnailImageUrl,
                ci.Price
            )).ToList()
        );

        return Ok(dto);
    }
}
