using InventoryAPI.DTOs;

namespace InventoryAPI.Service.Interface
{
    
        public interface ISupplierService
        {
            Task<IEnumerable<ReadSupplierDto>> GetAllAsync();
            Task<ReadSupplierDto?> GetByIdAsync(Guid id);
            Task<ReadSupplierDto> CreateAsync(CreateSupplierDto dto);
            Task<bool> UpdateAsync(Guid id, UpdateSupplierDto dto);
            Task<bool> DeleteAsync(Guid id); // soft delete
        }
    
}
