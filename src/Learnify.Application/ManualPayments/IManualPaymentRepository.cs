using Learnify.Domain.Entities;

namespace Learnify.Application.ManualPayments;

public interface IManualPaymentRepository
{
    // Payment Methods
    Task<List<ManualPaymentMethod>> GetActiveMethodsAsync();
    Task<List<ManualPaymentMethod>> GetAllMethodsAsync();
    Task<ManualPaymentMethod?> GetMethodByIdAsync(int id);
    Task<ManualPaymentMethod> CreateMethodAsync(ManualPaymentMethod method);
    Task<ManualPaymentMethod> UpdateMethodAsync(ManualPaymentMethod method);
    Task DeleteMethodAsync(int id);

    // Payment Requests
    Task<ManualPaymentRequest?> GetRequestByIdAsync(int id);
    Task<List<ManualPaymentRequest>> GetRequestsByUserIdAsync(string userId);
    Task<List<ManualPaymentRequest>> GetPendingRequestsAsync();
    Task<List<ManualPaymentRequest>> GetAllRequestsAsync(int page, int pageSize, string? status = null);
    Task<int> GetRequestsCountAsync(string? status = null);
    Task<ManualPaymentRequest> CreateRequestAsync(ManualPaymentRequest request);
    Task<ManualPaymentRequest> UpdateRequestAsync(ManualPaymentRequest request);

    // System Settings
    Task<bool> IsManualPaymentEnabledAsync();
    Task SetManualPaymentEnabledAsync(bool enabled);
    Task<bool> IsStripePaymentEnabledAsync();
    Task SetStripePaymentEnabledAsync(bool enabled);
    Task<string?> GetSettingAsync(string key);
    Task SetSettingAsync(string key, string value);
}
