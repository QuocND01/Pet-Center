using CustomerAPI.DTOs.Address;

namespace CustomerAPI.Service.Interface
{
    public interface IAddressService
    {
        Task<List<ViewAddressDto>> GetMyAddressesAsync(Guid customerId);
        Task<ViewAddressDto?> GetByIdAsync(Guid id);

        Task<ViewAddressDto> CreateAsync(Guid customerId, CreateAddressDto dto);
        Task<bool> UpdateAsync(Guid id, UpdateAddressDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
