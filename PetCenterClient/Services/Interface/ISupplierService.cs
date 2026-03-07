using PetCenterClient.DTOs;
namespace PetCenterClient.Services.Interface
    
{
    public interface ISupplierService
    {
        Task<List<ReadSupplierDto>> GetAllAsync();
        Task<ReadSupplierDto?> GetByIdAsync(Guid id);
        Task CreateAsync(CreateSupplierDto dto);
        Task UpdateAsync(Guid id, UpdateSupplierDto dto);
        Task DeleteAsync(Guid id);
    }
}
