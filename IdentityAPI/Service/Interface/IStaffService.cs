using IdentityAPI.DTOs.Response;
using IdentityAPI.DTOs.Resquest;

namespace IdentityAPI.Service.Interface
{
    public interface IStaffService
    {
        Task<IEnumerable<StaffResponseDto>> GetListAsync();
        Task<StaffResponseDto?> GetDetailsAsync(Guid id);
        Task<IEnumerable<StaffResponseDto>> SearchStaffAsync(string keyword);

        /// <summary>
        /// Tạo staff mới và tự động gán role "Staff" vào bảng StaffRoles.
        /// Trả về false nếu email đã tồn tại hoặc không tìm thấy role "Staff".
        /// </summary>
        Task<bool> CreateStaffAsync(StaffCreateDto dto);

        Task<bool> UpdateStaffAsync(Guid id, StaffUpdateDto dto);
        Task<bool> DeleteStaffAsync(Guid id);
    }
}