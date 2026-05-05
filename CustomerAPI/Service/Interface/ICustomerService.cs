using CustomerAPI.DTOs.Request;
using CustomerAPI.DTOs.Response;

namespace CustomerAPI.Service.Interface
{
    public interface ICustomerService
    {
        // ── Customer ───────────────────────────────────
        Task<CustomerProfileResponseDto?> GetProfileAsync(Guid customerId);
        Task<(bool Success, string Message)> UpdateProfileAsync(
            Guid customerId, UpdateCustomerProfileRequestDto request);

        // ── Staff/Admin ────────────────────────────────
        Task<List<CustomerResponseDto>> GetAllCustomersAsync();
        Task<CustomerResponseDto?> GetCustomerByIdAsync(Guid customerId);
        Task<bool> ChangeCustomerStatusAsync(Guid customerId, bool isActive);

        Task<CustomerInternalDto?> GetInternalAsync(Guid customerId);
    }
}
