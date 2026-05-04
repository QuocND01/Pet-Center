using VoucherAPI.DTOs.Request;
using VoucherAPI.DTOs.Response;

namespace VoucherAPI.Services.Intterfaces
{
    public interface IVoucherService
    {
        Task<IEnumerable<VoucherDto>> GetAllAsync();
        Task<VoucherDto?> GetByIdAsync(Guid id);
        Task<VoucherDto?> GetByCodeAsync(string code);
        Task<(bool Success, string Message, VoucherDto? Data)> CreateAsync(CreateVoucherDto dto);
        Task<(bool Success, string Message, VoucherDto? Data)> UpdateAsync(Guid id, UpdateVoucherDto dto);
        Task<(bool Success, string Message)> ToggleStatusAsync(Guid id, bool isActive);
    }
}
