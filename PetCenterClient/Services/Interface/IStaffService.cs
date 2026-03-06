using PetCenterClient.DTOs;

namespace PetCenterClient.Services.Interface
{
    public interface IStaffService
    {
        Task<List<StaffDto>> GetAllAsync();
        Task<StaffDto?> GetByIdAsync(Guid id);
        Task<bool> CreateAsync(StaffDto dto);
        Task<bool> UpdateAsync(Guid id, StaffDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}