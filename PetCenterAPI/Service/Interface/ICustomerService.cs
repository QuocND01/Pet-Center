using PetCenterAPI.DTOs.Requests.CustomerProfile;
using PetCenterAPI.DTOs.Responses.CustomerProfile;
using PetCenterAPI.DTOs.Responses.ManageCustomer;

namespace PetCenterAPI.Service.Interface
{
    public interface ICustomerService
    {
        // ============================================================
        // CUSTOMER — PROFILE
        // ============================================================
        Task<CustomerProfileResponseDTO?> GetProfileAsync(Guid customerId);

        // ============================================================
        // CUSTOMER — UPDATE PROFILE
        // ============================================================
        Task<(bool Success, string Message)> UpdateProfileAsync(
            Guid customerId, UpdateCustomerProfileRequestDTO request);
        // ============================================================
        // STAFF / ADMIN — CUSTOMER MANAGEMENT
        // ============================================================
        Task<List<CustomerResponseDTO>> GetAllCustomersAsync();
        Task<CustomerResponseDTO?> GetCustomerByIdAsync(Guid customerId);
    }
}
