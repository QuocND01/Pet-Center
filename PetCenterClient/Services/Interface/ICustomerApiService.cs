using PetCenterClient.DTOs;
using PetCenterClient.ViewModels.CustomerProfile;

namespace PetCenterClient.Services.Interface
{
    public interface ICustomerApiService
    {
        // ============================================================
        // CUSTOMER — VIEW PROFILE
        // ============================================================
        Task<CustomerProfileViewModel?> GetProfileAsync();
        Task<(bool Success, string Message)> UpdateProfileAsync(UpdateCustomerProfileViewModel dto);

        Task<List<CustomerListDto>> GetAllCustomersAsync();
        Task<CustomerDetailDto?> GetCustomerByIdAsync(Guid id);

        Task<bool> ChangeCustomerStatusAsync(Guid customerId, bool isActive);
        Task<string> GetDisplayNameAsync(Guid customerId);
    }
}
