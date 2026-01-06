using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using Learnify.Api.RateLimiting;
using Learnify.Application.ManualPayments;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Learnify.Api.Controllers;

/// <summary>
/// Admin controller for managing payment settings (Stripe and Manual payments)
/// </summary>
[ApiController]
    [ApiVersion("1.0")]
[Route("api/admin/payment-settings")]
[Authorize(Roles = "Admin")]
[EnableRateLimiting(RateLimitPolicies.Admin)]
public class PaymentSettingsAdminController : ControllerBase
{
    private readonly IManualPaymentRepository _repository;
    private readonly ILogger<PaymentSettingsAdminController> _logger;

    public PaymentSettingsAdminController(
        IManualPaymentRepository repository,
        ILogger<PaymentSettingsAdminController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Get all payment settings (Stripe and Manual payment enabled status)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaymentSettingsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaymentSettings()
    {
        var stripeEnabled = await _repository.IsStripePaymentEnabledAsync();
        var manualEnabled = await _repository.IsManualPaymentEnabledAsync();

        return Ok(new PaymentSettingsResponse
        {
            StripeEnabled = stripeEnabled,
            ManualPaymentEnabled = manualEnabled
        });
    }

    /// <summary>
    /// Toggle Stripe payment enabled/disabled
    /// </summary>
    [HttpPut("stripe")]
    [ProducesResponseType(typeof(PaymentSettingsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ToggleStripePayment([FromBody] TogglePaymentRequest request)
    {
        await _repository.SetStripePaymentEnabledAsync(request.Enabled);
        _logger.LogInformation("Stripe payment {Status} by admin", request.Enabled ? "enabled" : "disabled");

        var stripeEnabled = await _repository.IsStripePaymentEnabledAsync();
        var manualEnabled = await _repository.IsManualPaymentEnabledAsync();

        return Ok(new PaymentSettingsResponse
        {
            StripeEnabled = stripeEnabled,
            ManualPaymentEnabled = manualEnabled
        });
    }

    /// <summary>
    /// Toggle Manual payment enabled/disabled
    /// </summary>
    [HttpPut("manual")]
    [ProducesResponseType(typeof(PaymentSettingsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ToggleManualPayment([FromBody] TogglePaymentRequest request)
    {
        await _repository.SetManualPaymentEnabledAsync(request.Enabled);
        _logger.LogInformation("Manual payment {Status} by admin", request.Enabled ? "enabled" : "disabled");

        var stripeEnabled = await _repository.IsStripePaymentEnabledAsync();
        var manualEnabled = await _repository.IsManualPaymentEnabledAsync();

        return Ok(new PaymentSettingsResponse
        {
            StripeEnabled = stripeEnabled,
            ManualPaymentEnabled = manualEnabled
        });
    }
}

/// <summary>
/// Response for payment settings
/// </summary>
public class PaymentSettingsResponse
{
    [JsonPropertyName("stripeEnabled")]
    public bool StripeEnabled { get; set; }

    [JsonPropertyName("manualPaymentEnabled")]
    public bool ManualPaymentEnabled { get; set; }
}

/// <summary>
/// Request to toggle payment method
/// </summary>
public class TogglePaymentRequest
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }
}
