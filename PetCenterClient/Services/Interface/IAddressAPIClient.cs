using PetCenterClient.ViewModels;

namespace PetCenterClient.Services.Interface
{
    public interface IAddressAPIClient
    {
        Task<List<ReadAddressViewModel>?> GetMyAddressesAsync();
        Task<System.Net.Http.HttpResponseMessage> AddAddressAsync(MutateAddressViewModel dto);
        Task<System.Net.Http.HttpResponseMessage> UpdateAddressAsync(Guid id, MutateAddressViewModel dto);
        Task<System.Net.Http.HttpResponseMessage> DeleteAddressAsync(Guid id);
    }
}