using AddressAPI.Models;
using AddressAPI.Repository.Interface;
using AutoMapper;

namespace AddressAPI.Service.Interface
{
    public interface IAddressService
    {
        Task<IEnumerable<AddressResponseDTO>> GetAddressesAsync();
        Task<AddressResponseDTO?> GetAddressByIdAsync(Guid id);
        Task<AddressResponseDTO> CreateAddressAsync(AddressCreateDTO dto);
        Task<bool> UpdateAddressAsync(Guid id, AddressCreateDTO dto);
        Task<bool> DeleteAddressAsync(Guid id);
    }

}
