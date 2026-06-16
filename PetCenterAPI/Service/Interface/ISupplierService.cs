using PetCenterAPI.DTOs;
using PetCenterAPI.DTOs.Responses.Supplier;

namespace PetCenterAPI.Service.Interface
{
    
        public interface ISupplierService
        {
            Task<IEnumerable<ViewSupplierResponseDTO>> GetAllAsync();
            Task<ViewSupplierResponseDTO?> GetByIdAsync(Guid id);
            Task<ViewSupplierResponseDTO> CreateAsync(CreateSupplierRequestDTO dto);
            Task<bool> UpdateAsync(Guid id, CreateSupplierRequestDTO dto);
            Task<bool> DeleteAsync(Guid id); // soft delete
        }
    
}
