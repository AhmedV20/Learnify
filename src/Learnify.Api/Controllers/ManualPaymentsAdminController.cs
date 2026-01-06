using Asp.Versioning;
using Learnify.Api.RateLimiting;
using Learnify.Application.ManualPayments;
using Learnify.Application.ManualPayments.DTOs;
using Learnify.Application.Common.Interfaces;
using Learnify.Application.Email;
using Learnify.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Learnify.Api.Controllers;

/// <summary>
/// Admin endpoints for manual payment management
/// </summary>
[ApiController]
    [ApiVersion("1.0")]
[EnableRateLimiting(RateLimitPolicies.Admin)]
[Route("api/v{version:apiVersion}/admin/manual-payments")]
[Authorize(Roles = "Admin")]
public class ManualPaymentsAdminController : ControllerBase
{
    private readonly IManualPaymentRepository _repository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;

    public ManualPaymentsAdminController(
        IManualPaymentRepository repository,
        IEnrollmentRepository enrollmentRepository,
        UserManager<ApplicationUser> userManager,
        IEmailService emailService)
    {
        _repository = repository;
        _enrollmentRepository = enrollmentRepository;
        _userManager = userManager;
        _emailService = emailService;
    }

    #region Settings

    /// <summary>
    /// Get manual payment settings (admin view)
    /// </summary>
    [HttpGet("settings")]
    public async Task<ActionResult<ManualPaymentSettingsDto>> GetSettings()
    {
        var isEnabled = await _repository.IsManualPaymentEnabledAsync();
        return Ok(new ManualPaymentSettingsDto(isEnabled));
    }

    /// <summary>
    /// Toggle manual payment enabled/disabled
    /// </summary>
    [HttpPut("settings")]
    public async Task<ActionResult<ManualPaymentSettingsDto>> UpdateSettings([FromBody] ManualPaymentSettingsDto settings)
    {
        await _repository.SetManualPaymentEnabledAsync(settings.IsEnabled);
        return Ok(settings);
    }

    #endregion

    #region Payment Methods

    /// <summary>
    /// Get all payment methods (including inactive)
    /// </summary>
    [HttpGet("methods")]
    public async Task<ActionResult<List<ManualPaymentMethodAdminDto>>> GetAllMethods()
    {
        var methods = await _repository.GetAllMethodsAsync();
        var dtos = methods.Select(m => new ManualPaymentMethodAdminDto(
            m.Id,
            m.Name,
            m.NameAr,
            m.Type,
            m.AccountIdentifier,
            m.AccountName,
            m.Instructions,
            m.InstructionsAr,
            m.IconUrl,
            m.IsActive,
            m.DisplayOrder,
            m.CreatedAt,
            m.UpdatedAt
        )).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// Create a payment method
    /// </summary>
    [HttpPost("methods")]
    public async Task<ActionResult<ManualPaymentMethodAdminDto>> CreateMethod([FromBody] CreatePaymentMethodRequest request)
    {
        var method = new ManualPaymentMethod
        {
            Name = request.Name,
            NameAr = request.NameAr,
            Type = request.Type,
            AccountIdentifier = request.AccountIdentifier,
            AccountName = request.AccountName,
            Instructions = request.Instructions,
            InstructionsAr = request.InstructionsAr,
            IconUrl = request.IconUrl,
            IsActive = request.IsActive,
            DisplayOrder = request.DisplayOrder
        };

        var created = await _repository.CreateMethodAsync(method);

        var dto = new ManualPaymentMethodAdminDto(
            created.Id,
            created.Name,
            created.NameAr,
            created.Type,
            created.AccountIdentifier,
            created.AccountName,
            created.Instructions,
            created.InstructionsAr,
            created.IconUrl,
            created.IsActive,
            created.DisplayOrder,
            created.CreatedAt,
            created.UpdatedAt
        );

        return CreatedAtAction(nameof(GetAllMethods), dto);
    }

    /// <summary>
    /// Update a payment method
    /// </summary>
    [HttpPut("methods/{id}")]
    public async Task<ActionResult<ManualPaymentMethodAdminDto>> UpdateMethod(int id, [FromBody] CreatePaymentMethodRequest request)
    {
        var method = await _repository.GetMethodByIdAsync(id);
        if (method == null)
        {
            return NotFound();
        }

        method.Name = request.Name;
        method.NameAr = request.NameAr;
        method.Type = request.Type;
        method.AccountIdentifier = request.AccountIdentifier;
        method.AccountName = request.AccountName;
        method.Instructions = request.Instructions;
        method.InstructionsAr = request.InstructionsAr;
        method.IconUrl = request.IconUrl;
        method.IsActive = request.IsActive;
        method.DisplayOrder = request.DisplayOrder;

        var updated = await _repository.UpdateMethodAsync(method);

        var dto = new ManualPaymentMethodAdminDto(
            updated.Id,
            updated.Name,
            updated.NameAr,
            updated.Type,
            updated.AccountIdentifier,
            updated.AccountName,
            updated.Instructions,
            updated.InstructionsAr,
            updated.IconUrl,
            updated.IsActive,
            updated.DisplayOrder,
            updated.CreatedAt,
            updated.UpdatedAt
        );

        return Ok(dto);
    }

    /// <summary>
    /// Delete a payment method
    /// </summary>
    [HttpDelete("methods/{id}")]
    public async Task<ActionResult> DeleteMethod(int id)
    {
        var method = await _repository.GetMethodByIdAsync(id);
        if (method == null)
        {
            return NotFound();
        }

        await _repository.DeleteMethodAsync(id);
        return NoContent();
    }

    #endregion

    #region Payment Requests

    /// <summary>
    /// Get pending payment requests
    /// </summary>
    [HttpGet("pending")]
    public async Task<ActionResult<List<ManualPaymentRequestAdminDto>>> GetPendingRequests()
    {
        var requests = await _repository.GetPendingRequestsAsync();
        var dtos = requests.Select(MapToAdminDto).ToList();
        return Ok(dtos);
    }

    /// <summary>
    /// Get all payment requests with pagination
    /// </summary>
    [HttpGet("requests")]
    public async Task<ActionResult<object>> GetAllRequests(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null)
    {
        var requests = await _repository.GetAllRequestsAsync(page, pageSize, status);
        var totalCount = await _repository.GetRequestsCountAsync(status);
        var dtos = requests.Select(MapToAdminDto).ToList();

        return Ok(new
        {
            items = dtos,
            totalCount,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
    }

    /// <summary>
    /// Get a specific payment request
    /// </summary>
    [HttpGet("requests/{id}")]
    public async Task<ActionResult<ManualPaymentRequestAdminDto>> GetRequest(int id)
    {
        var request = await _repository.GetRequestByIdAsync(id);
        if (request == null)
        {
            return NotFound();
        }

        return Ok(MapToAdminDto(request));
    }

    /// <summary>
    /// Approve a payment request
    /// </summary>
    [HttpPost("requests/{id}/approve")]
    public async Task<ActionResult<ManualPaymentRequestAdminDto>> ApproveRequest(int id, [FromBody] ReviewPaymentRequest review)
    {
        var request = await _repository.GetRequestByIdAsync(id);
        if (request == null)
        {
            return NotFound();
        }

        if (request.Status != "Pending")
        {
            return BadRequest("This request has already been reviewed");
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Update request status
        request.Status = "Approved";
        request.AdminNote = review.AdminNote;
        request.ReviewedByAdminId = adminId;
        request.ReviewedAt = DateTime.UtcNow;

        await _repository.UpdateRequestAsync(request);

        // Enroll user in all courses
        foreach (var item in request.CartItems)
        {
            // Check if not already enrolled
            var existingEnrollment = await _enrollmentRepository.GetEnrollmentByUserAndCourseAsync(request.UserId, item.CourseId);
            if (existingEnrollment == null)
            {
                var enrollment = new Enrollment
                {
                    UserId = request.UserId,
                    CourseId = item.CourseId,
                    EnrollmentDate = DateTime.UtcNow,
                    PricePaid = item.Price
                };
                await _enrollmentRepository.AddEnrollmentAsync(enrollment);
            }
        }

        // Send approval email
        if (request.User != null)
        {
            await SendApprovalEmailAsync(request);
        }

        // Reload to get updated data
        var updated = await _repository.GetRequestByIdAsync(id);
        return Ok(MapToAdminDto(updated!));
    }

    /// <summary>
    /// Reject a payment request
    /// </summary>
    [HttpPost("requests/{id}/reject")]
    public async Task<ActionResult<ManualPaymentRequestAdminDto>> RejectRequest(int id, [FromBody] ReviewPaymentRequest review)
    {
        var request = await _repository.GetRequestByIdAsync(id);
        if (request == null)
        {
            return NotFound();
        }

        if (request.Status != "Pending")
        {
            return BadRequest("This request has already been reviewed");
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Update request status
        request.Status = "Rejected";
        request.AdminNote = review.AdminNote;
        request.ReviewedByAdminId = adminId;
        request.ReviewedAt = DateTime.UtcNow;

        await _repository.UpdateRequestAsync(request);

        // Send rejection email
        if (request.User != null)
        {
            await SendRejectionEmailAsync(request);
        }

        // Reload to get updated data
        var updated = await _repository.GetRequestByIdAsync(id);
        return Ok(MapToAdminDto(updated!));
    }

    #endregion

    #region Private Helpers

    private ManualPaymentRequestAdminDto MapToAdminDto(ManualPaymentRequest r)
    {
        return new ManualPaymentRequestAdminDto(
            r.Id,
            r.UserId,
            r.User?.FirstName + " " + r.User?.LastName,
            r.User?.Email ?? "",
            r.PaymentMethodId,
            r.PaymentMethod?.Name ?? "",
            r.Amount,
            r.ProofImageUrl,
            r.UserMessage,
            r.Status,
            r.AdminNote,
            r.ReviewedByAdminId,
            r.ReviewedByAdmin != null ? r.ReviewedByAdmin.FirstName + " " + r.ReviewedByAdmin.LastName : null,
            r.CreatedAt,
            r.ReviewedAt,
            r.CartItems.Select(ci => new ManualPaymentCartItemDto(
                ci.CourseId,
                ci.Course?.Title ?? "",
                ci.Course?.ThumbnailImageUrl,
                ci.Price
            )).ToList()
        );
    }

    private async Task SendApprovalEmailAsync(ManualPaymentRequest request)
    {
        if (request.User?.Email == null) return;

        var courseNames = request.CartItems.Select(ci => ci.Course?.Title ?? "Unknown Course").ToList();
        
        await _emailService.SendPaymentApprovedEmailAsync(
            request.User.Email,
            request.User.FirstName ?? "User",
            request.Amount,
            courseNames,
            request.AdminNote
        );
    }

    private async Task SendRejectionEmailAsync(ManualPaymentRequest request)
    {
        if (request.User?.Email == null) return;

        await _emailService.SendPaymentRejectedEmailAsync(
            request.User.Email,
            request.User.FirstName ?? "User",
            request.Amount,
            request.AdminNote
        );
    }

    #endregion
}
