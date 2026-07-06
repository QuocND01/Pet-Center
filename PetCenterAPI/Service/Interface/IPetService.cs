using static PetCenterAPI.DTOs.Requests.CustomerProfile.PetRequestDTO;

namespace PetCenterAPI.Service.Interface
{
    public interface IPetService
    {
        Task<List<ReadPetListDTO>> GetMyPetsAsync(Guid customerId);
        Task<ReadPetDetailDTO?> GetPetDetailsAsync(Guid petId, Guid customerId);
    }
}