using Learnify.Application.ManualPayments;
using Learnify.Domain.Entities;
using Learnify.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Learnify.Infrastructure.ManualPayments;

public class ManualPaymentRepository : IManualPaymentRepository
{
    private readonly ApplicationDbContext _context;
    private const string ManualPaymentEnabledKey = "ManualPaymentEnabled";
    private const string StripePaymentEnabledKey = "StripePaymentEnabled";

    public ManualPaymentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Payment Methods

    public async Task<List<ManualPaymentMethod>> GetActiveMethodsAsync()
    {
        return await _context.ManualPaymentMethods
            .Where(m => m.IsActive)
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync();
    }

    public async Task<List<ManualPaymentMethod>> GetAllMethodsAsync()
    {
        return await _context.ManualPaymentMethods
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync();
    }

    public async Task<ManualPaymentMethod?> GetMethodByIdAsync(int id)
    {
        return await _context.ManualPaymentMethods.FindAsync(id);
    }

    public async Task<ManualPaymentMethod> CreateMethodAsync(ManualPaymentMethod method)
    {
        method.CreatedAt = DateTime.UtcNow;
        _context.ManualPaymentMethods.Add(method);
        await _context.SaveChangesAsync();
        return method;
    }

    public async Task<ManualPaymentMethod> UpdateMethodAsync(ManualPaymentMethod method)
    {
        method.UpdatedAt = DateTime.UtcNow;
        _context.ManualPaymentMethods.Update(method);
        await _context.SaveChangesAsync();
        return method;
    }

    public async Task DeleteMethodAsync(int id)
    {
        var method = await _context.ManualPaymentMethods.FindAsync(id);
        if (method != null)
        {
            _context.ManualPaymentMethods.Remove(method);
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Payment Requests

    public async Task<ManualPaymentRequest?> GetRequestByIdAsync(int id)
    {
        return await _context.ManualPaymentRequests
            .Include(r => r.User)
            .Include(r => r.PaymentMethod)
            .Include(r => r.ReviewedByAdmin)
            .Include(r => r.CartItems)
                .ThenInclude(ci => ci.Course)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<List<ManualPaymentRequest>> GetRequestsByUserIdAsync(string userId)
    {
        return await _context.ManualPaymentRequests
            .Include(r => r.PaymentMethod)
            .Include(r => r.CartItems)
                .ThenInclude(ci => ci.Course)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<ManualPaymentRequest>> GetPendingRequestsAsync()
    {
        return await _context.ManualPaymentRequests
            .Include(r => r.User)
            .Include(r => r.PaymentMethod)
            .Include(r => r.CartItems)
                .ThenInclude(ci => ci.Course)
            .Where(r => r.Status == "Pending")
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<ManualPaymentRequest>> GetAllRequestsAsync(int page, int pageSize, string? status = null)
    {
        var query = _context.ManualPaymentRequests
            .Include(r => r.User)
            .Include(r => r.PaymentMethod)
            .Include(r => r.ReviewedByAdmin)
            .Include(r => r.CartItems)
                .ThenInclude(ci => ci.Course)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(r => r.Status == status);
        }

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetRequestsCountAsync(string? status = null)
    {
        var query = _context.ManualPaymentRequests.AsQueryable();
        
        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(r => r.Status == status);
        }

        return await query.CountAsync();
    }

    public async Task<ManualPaymentRequest> CreateRequestAsync(ManualPaymentRequest request)
    {
        request.CreatedAt = DateTime.UtcNow;
        request.Status = "Pending";
        _context.ManualPaymentRequests.Add(request);
        await _context.SaveChangesAsync();
        return request;
    }

    public async Task<ManualPaymentRequest> UpdateRequestAsync(ManualPaymentRequest request)
    {
        _context.ManualPaymentRequests.Update(request);
        await _context.SaveChangesAsync();
        return request;
    }

    #endregion

    #region System Settings

    public async Task<bool> IsManualPaymentEnabledAsync()
    {
        var setting = await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.Key == ManualPaymentEnabledKey);
        
        return setting?.Value?.ToLower() == "true";
    }

    public async Task SetManualPaymentEnabledAsync(bool enabled)
    {
        await SetSettingAsync(ManualPaymentEnabledKey, enabled.ToString().ToLower());
    }

    public async Task<bool> IsStripePaymentEnabledAsync()
    {
        var setting = await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.Key == StripePaymentEnabledKey);
        
        // Default to true if not set (Stripe enabled by default)
        return setting == null || setting.Value?.ToLower() != "false";
    }

    public async Task SetStripePaymentEnabledAsync(bool enabled)
    {
        await SetSettingAsync(StripePaymentEnabledKey, enabled.ToString().ToLower());
    }

    public async Task<string?> GetSettingAsync(string key)
    {
        var setting = await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.Key == key);
        return setting?.Value;
    }

    public async Task SetSettingAsync(string key, string value)
    {
        var setting = await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.Key == key);

        if (setting == null)
        {
            setting = new SystemSetting
            {
                Key = key,
                Value = value,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.SystemSettings.Add(setting);
        }
        else
        {
            setting.Value = value;
            setting.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    #endregion
}
