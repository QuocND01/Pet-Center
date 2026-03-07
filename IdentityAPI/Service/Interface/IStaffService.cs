using IdentityAPI.DTOs.Response;
using IdentityAPI.DTOs.Resquest;

namespace IdentityAPI.Service.Interface
{
    public interface IStaffService
    {
        Task<IEnumerable<StaffResponseDto>> GetListAsync();
        Task<StaffResponseDto?> GetDetailsAsync(Guid id);
        Task<IEnumerable<StaffResponseDto>> SearchStaffAsync(string keyword);
        Task<bool> CreateStaffAsync(StaffCreateDto dto);
        Task<bool> UpdateStaffAsync(Guid id, StaffUpdateDto dto);
        Task<bool> DeleteStaffAsync(Guid id);
    }
}