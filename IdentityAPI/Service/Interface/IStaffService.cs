using IdentityAPI.DTOs.Request;
using IdentityAPI.DTOs.Response;

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