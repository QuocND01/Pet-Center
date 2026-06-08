using PetCenterAPI.DTOs;

namespace PetCenterAPI.Service.Interface
{
    
        public interface ISupplierService
        {
            Task<IEnumerable<ReadSupplierDto>> GetAllAsync();
            Task<ReadSupplierDto?> GetByIdAsync(Guid id);
            Task<ReadSupplierDto> CreateAsync(WriteSupplierDto dto);
            Task<bool> UpdateAsync(Guid id, WriteSupplierDto dto);
            Task<bool> DeleteAsync(Guid id); // soft delete
        }
    
}
