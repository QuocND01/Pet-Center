using PetCenterAPI.Models;
using static PetCenterAPI.DTOs.Requests.CustomerProfile.AddressRequestDTO;

namespace PetCenterAPI.Service.Interface
{
    public interface IAddressService
    {
        Task<List<ReadAddressDTO>> GetCustomerAddressesAsync(Guid customerId);
        Task<bool> AddAddressAsync(Guid customerId, MutateAddressDTO dto);
        Task<bool> UpdateAddressAsync(Guid customerId, Guid addressId, MutateAddressDTO dto);
        Task<bool> DeleteAddressAsync(Guid customerId, Guid addressId);
    }
}