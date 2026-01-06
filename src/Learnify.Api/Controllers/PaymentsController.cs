using Asp.Versioning;
using Learnify.Application.Payments.Commands.CreateCheckoutSession;
using Learnify.Application.Payments.Commands.ProcessPaymentSuccess;
using Learnify.Application.Payments.DTOs.Requests;
using Learnify.Application.Payments.DTOs.Responses;
using Learnify.Application.Common.Interfaces;
using Learnify.Application.ManualPayments;
using Learnify.Api.RateLimiting;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Learnify.Api.Controllers;

// Endpoint for payments
// POST /api/payments/checkout-session
// POST /api/payments/verify-payment/{sessionId}
// GET /api/payments/my-payments
// GET /api/payments/settings

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
    [ApiVersion("1.0")]
[Authorize]
[EnableRateLimiting(RateLimitPolicies.Payment)]
public class PaymentsController(
    IMediator _mediator, 
    IStripeService _stripeService, 
    IPaymentRepository _paymentRepository,
    IManualPaymentRepository _settingsRepository) : ControllerBase
{
    private string GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    /// <summary>
    /// Get payment settings for frontend (which payment methods are enabled)
    /// </summary>
    [HttpGet("settings")]
    public async Task<IActionResult> GetPaymentSettings()
    {
        var stripeEnabled = await _settingsRepository.IsStripePaymentEnabledAsync();
        var manualEnabled = await _settingsRepository.IsManualPaymentEnabledAsync();

        return Ok(new { stripeEnabled, manualEnabled });
    }

    [HttpPost("checkout-session")]
    public async Task<ActionResult<CreateCheckoutSessionResponse>> CreateCheckoutSession([FromBody] CreateCheckoutSessionRequest request)
    {
        // Check if Stripe payment is enabled
        var stripeEnabled = await _settingsRepository.IsStripePaymentEnabledAsync();
        if (!stripeEnabled)
        {
            return BadRequest(new { error = "Stripe payment is currently disabled" });
        }

        var userId = GetCurrentUserId();
        var command = new CreateCheckoutSessionCommand(userId, request.SuccessUrl, request.CancelUrl);
        
        var (checkoutUrl, sessionId) = await _mediator.Send(command);
        
        var response = new CreateCheckoutSessionResponse { CheckoutUrl = checkoutUrl, SessionId = sessionId };
        return Ok(response);
    }

    // This endpoint is used to verify if the payment was successful and process the payment
    // So the command will be called ProcessPaymentSuccessCommand
    [HttpPost("verify-payment/{sessionId}")]
    public async Task<ActionResult<VerifyPaymentResponse>> VerifyAndProcessPayment(string sessionId)
    {
        var userId = GetCurrentUserId();
        
        // Check if payment was successful
        var isSuccessful = await _stripeService.IsSessionSuccessfulAsync(sessionId);
        
        if (!isSuccessful)
        {
            return Ok(new VerifyPaymentResponse 
            { 
                Success = false, 
                Message = "Payment not completed yet" 
            });
        }

        // Check if already processed (prevent duplicate processing)
        var existingPayment = await _paymentRepository.GetPaymentByTransactionIdAsync(sessionId);
        if (existingPayment != null)
        {
            return Ok(new VerifyPaymentResponse 
            { 
                Success = true, 
                Message = "Payment already processed", 
                AlreadyProcessed = true,
                SessionId = sessionId
            });
        }

        // Process the payment
        var command = new ProcessPaymentSuccessCommand(sessionId, userId);
        var (success, successUrl) = await _mediator.Send(command);
        
        if (success)
        {
            return Ok(new VerifyPaymentResponse 
            { 
                Success = true, 
                Message = "Payment processed successfully! You are now enrolled in your courses.",
                SuccessUrl = successUrl,
                SessionId = sessionId
            });
        }
        
        return StatusCode(500, new VerifyPaymentResponse 
        { 
            Success = false, 
            Message = "Error processing payment" 
        });
    }

    [HttpGet("my-payments")]
    public async Task<ActionResult> GetMyPayments()
    {
        var userId = GetCurrentUserId();
        var payments = await _paymentRepository.GetPaymentsByUserIdAsync(userId);
        
        return Ok(payments);
    }
} 