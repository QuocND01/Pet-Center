using PetCenterClient.DTOs;

namespace PetCenterClient.Services.Interface
{
    public interface ICustomerService
    {
        Task<CustomerProfileResponseDto?> GetProfileAsync();
        Task<bool> UpdateProfileAsync(UpdateCustomerProfileRequestDto dto);
    }
}
