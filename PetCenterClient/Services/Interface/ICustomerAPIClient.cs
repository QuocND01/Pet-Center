using PetCenterClient.DTOs;

namespace PetCenterClient.Services.Interface
{
    public interface ICustomerAPIClient
    {
        Task<CustomerProfileResponseDto?> GetProfileAsync();
        Task<(bool Success, string Message)> UpdateProfileAsync(UpdateCustomerProfileRequestDto dto);

        Task<List<CustomerListDto>> GetAllCustomersAsync();
        Task<CustomerDetailDto?> GetCustomerByIdAsync(Guid id);

        Task<bool> ChangeCustomerStatusAsync(Guid customerId, bool isActive);
        Task<string> GetDisplayNameAsync(Guid customerId);
    }
}
