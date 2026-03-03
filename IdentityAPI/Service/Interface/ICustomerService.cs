using IdentityAPI.DTOs.Response;

namespace IdentityAPI.Service.Interface
{
    public interface ICustomerService
    {
        Task<List<CustomerResponseDto>> GetAllCustomersAsync();

        Task<CustomerResponseDto?> GetCustomerByIdAsync(Guid customerId);
    }
}
