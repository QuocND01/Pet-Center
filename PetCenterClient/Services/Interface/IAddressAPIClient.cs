using PetCenterClient.DTOs;

namespace PetCenterClient.Services.Interface
{
    public interface IAddressAPIClient
    {
        Task<List<ReadAddressViewModel>?> GetMyAddressesAsync();
        Task<bool> AddAddressAsync(MutateAddressViewModel dto);
        Task<bool> UpdateAddressAsync(Guid id, MutateAddressViewModel dto);
        Task<bool> DeleteAddressAsync(Guid id);
    }
}