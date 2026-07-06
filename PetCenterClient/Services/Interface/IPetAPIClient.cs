using PetCenterClient.DTOs;

namespace PetCenterClient.Services.Interface
{
    public interface IPetAPIClient
    {
        Task<List<ReadPetListViewModel>?> GetMyPetsAsync();
        Task<ReadPetDetailViewModel?> GetPetDetailsAsync(Guid id);
    }
}