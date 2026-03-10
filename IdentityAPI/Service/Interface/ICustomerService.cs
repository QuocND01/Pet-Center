using IdentityAPI.DTOs.Response;
using IdentityAPI.DTOs.Resquest;

namespace IdentityAPI.Service.Interface
{
    public interface ICustomerService
    {
        Task<List<CustomerResponseDto>> GetAllCustomersAsync();

        Task<CustomerResponseDto?> GetCustomerByIdAsync(Guid customerId);

        Task<CustomerProfileResponseDto?> GetProfileAsync(Guid customerId);

        Task<bool> UpdateProfileAsync(Guid customerId, UpdateCustomerProfileRequestDto request);

        Task<bool> ChangeCustomerStatusAsync(Guid customerId, bool isActive);
    }
}
