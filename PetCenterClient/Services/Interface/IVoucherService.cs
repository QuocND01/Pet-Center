using PetCenterClient.DTOs;

namespace PetCenterClient.Services.Interface
{
    public interface IVoucherService
    {
        Task<List<VoucherDto>> GetAllAsync();
        Task<VoucherDto?> GetByIdAsync(Guid id);
        Task<(bool Success, string Message, VoucherDto? Data)> CreateAsync(CreateVoucherDto dto);
        Task<(bool Success, string Message, VoucherDto? Data)> UpdateAsync(Guid id, UpdateVoucherDto dto);
        Task<(bool Success, string Message)> ToggleStatusAsync(Guid id, bool isActive);
    }
}
