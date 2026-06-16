using PetCenterClient.DTOs;
using PetCenterClient.ViewModels.CustomerProfile;
using PetCenterClient.ViewModels.ManageCustomer;

namespace PetCenterClient.Services.Interface
{
    public interface ICustomerApiService
    {
        // ============================================================
        // CUSTOMER — PROFILE
        // ============================================================
        Task<CustomerProfileViewModel?> GetProfileAsync();
        Task<(bool Success, string Message)> UpdateProfileAsync(UpdateCustomerProfileViewModel dto);

        // ============================================================
        // STAFF / ADMIN — Manage CUSTOMER
        // ============================================================
        Task<List<CustomerListViewModel>> GetAllCustomersAsync();
        Task<CustomerDetailViewModel?> GetCustomerByIdAsync(Guid id);

        Task<bool> ChangeCustomerStatusAsync(Guid customerId, bool isActive);
        Task<string> GetDisplayNameAsync(Guid customerId);
    }
}
