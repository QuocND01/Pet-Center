using PetCenterAPI.DTOs.Requests.CustomerProfile;
using PetCenterAPI.DTOs.Responses.CustomerProfile;

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
    }
}
