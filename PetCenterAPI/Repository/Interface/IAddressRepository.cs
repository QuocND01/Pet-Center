using PetCenterAPI.Models;

namespace PetCenterAPI.Repository.Interface
{
    public interface IAddressRepository
    {
        Task<List<Address>> GetAddressesByCustomerIdAsync(Guid customerId);
        Task<Address?> GetAddressByIdAsync(Guid addressId, Guid customerId);
        Task AddAddressAsync(Address address);
        Task UpdateAddressAsync(Address address);
        Task DeleteAddressAsync(Address address);
        Task ResetDefaultAddressAsync(Guid customerId);
        Task SaveAsync();
    }
}