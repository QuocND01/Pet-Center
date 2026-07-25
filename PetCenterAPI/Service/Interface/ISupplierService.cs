using PetCenterAPI.DTOs;
using PetCenterAPI.DTOs.Requests.Supplier;
using PetCenterAPI.DTOs.Responses.Supplier;

namespace PetCenterAPI.Service.Interface
{
    
        public interface ISupplierService
        {
            Task<IEnumerable<ReadSupplierResponseDTO>> GetAllAsync();
            Task<ReadSupplierResponseDTO?> GetByIdAsync(Guid id);
            
            Task<ReadSupplierResponseDTO> CreateAsync(CreateSupplierRequestDTO dto);
            Task<bool> UpdateAsync(Guid id, UpdateSupplierRequestDTO dto);
            Task<bool> DeleteAsync(Guid id); // soft delete
        }
    
}
