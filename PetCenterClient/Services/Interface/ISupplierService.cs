using PetCenterClient.DTOs;
namespace PetCenterClient.Services.Interface
    
{
    public interface ISupplierService
    {
        
            Task<List<ReadSupplierDto>> GetAllAsync();
            Task<ReadSupplierDto?> GetByIdAsync(Guid id);
            Task<ReadSupplierDto> CreateAsync(CreateSupplierDto dto); // Phải trả về DTO sau khi tạo
            Task<bool> UpdateAsync(Guid id, UpdateSupplierDto dto);   // Trả về bool để biết thành công hay không
            Task<bool> DeleteAsync(Guid id);                         // Trả về bool

            Task<List<SupplierSelectDto>> GetSupplierSelectAsync();
    }
}
