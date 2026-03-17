using PetCenterClient.DTOs;

namespace PetCenterClient.Services.Interface
{
    public interface IStaffService
    {
        Task<OdataResponse<StaffDto>> GetAllODataAsync(
            string? search = null,
            bool? isActive = null,
            string? sortBy = null,
            string sortOrder = "asc",
            int page = 1,
            int pageSize = 10);

        Task<List<StaffDto>> GetAllAsync();
        Task<StaffDto?> GetByIdAsync(Guid id);
        Task<bool> CreateAsync(StaffDto dto);
        Task<bool> UpdateAsync(Guid id, StaffDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<List<StaffNameListDto>> GetStaffNameListAsync();
    }
}