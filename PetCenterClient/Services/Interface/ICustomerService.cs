using PetCenterClient.DTOs;

namespace PetCenterClient.Services.Interface
{
    public interface ICustomerService
    {
        Task<CustomerProfileResponseDto?> GetProfileAsync();
        Task<(bool Success, string Message)> UpdateProfileAsync(UpdateCustomerProfileRequestDto dto);

        Task<List<CustomerListDto>> GetAllCustomersAsync();
        Task<CustomerDetailDto?> GetCustomerByIdAsync(Guid id);

        Task<bool> ChangeCustomerStatusAsync(Guid customerId, bool isActive);
    }
}
