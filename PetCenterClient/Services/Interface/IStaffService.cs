using PetCenterClient.DTOs;

namespace PetCenterClient.Services.Interface
{
    public interface IStaffService
    {
        Task<List<StaffListItemDto>> GetAllAsync();
        Task<StaffDetailDto?> GetByIdAsync(Guid id);
        Task<List<RoleDto>> GetRolesAsync();

        Task<(bool Success, string Message)> CreateAsync(CreateStaffDto dto);
        Task<(bool Success, string Message)> UpdateAsync(Guid id, UpdateStaffDto dto);
        Task<(bool Success, string Message)> DeleteAsync(Guid id);

        /// <summary>Lightweight name list for other modules (e.g. import stocks).</summary>
        Task<List<StaffNameListDto>> GetStaffNameListAsync();
    }
}
