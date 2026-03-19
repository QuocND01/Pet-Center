using PetCenterClient.DTOs;

namespace PetCenterClient.Services.Interface
{
    public interface IVoucherService
    {
        Task<List<VoucherDTO>> GetAllAsync();
        Task<VoucherDTO?> GetByIdAsync(Guid id);
        Task CreateAsync(CreateVoucherDTO dto);
        Task DeleteAsync(Guid id);

        Task<object> ApplyVoucherAsync(ApplyVoucherDTO dto);
        Task UpdateAsync(Guid id, CreateVoucherDTO dto);
        Task<List<VoucherDTO>> SearchAsync(string code);
    }
}
