using PetCenterAPI.DTOs.Requests.ManageStaff;
using PetCenterAPI.DTOs.Responses.ManageStaff;

namespace PetCenterAPI.Service.Interface
{
    /// <summary>Business logic for staff management.</summary>
    public interface IStaffService
    {
        Task<List<StaffListItemResponseDTO>> GetAllAsync();
        Task<StaffDetailResponseDTO?> GetByIdAsync(Guid staffId);
        Task<List<RoleResponseDTO>> GetAssignableRolesAsync();

        Task<(bool Success, string Message, Guid? StaffId)> CreateAsync(CreateStaffRequestDTO request);
        Task<(bool Success, string Message)> UpdateAsync(Guid staffId, UpdateStaffRequestDTO request);

        /// <summary>Soft delete: sets IsActive = false.</summary>
        Task<(bool Success, string Message)> DeleteAsync(Guid staffId);
    }
}
