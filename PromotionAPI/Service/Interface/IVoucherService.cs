using PromotionAPI.DTOs;

namespace PromotionAPI.Service.Interface
{
    public interface IVoucherService
    {
        Task<List<VoucherResponseDTO>> GetAllAsync();
        Task<VoucherResponseDTO?> GetByIdAsync(Guid id);
        Task CreateAsync(CreateVoucherDTO dto);
        Task DeleteAsync(Guid id);
        Task<object> ApplyVoucherAsync(ApplyVoucherDTO dto);
        Task UpdateAsync(Guid id, CreateVoucherDTO dto);
        Task<List<VoucherResponseDTO>> SearchAsync(string? code);
    }
}
