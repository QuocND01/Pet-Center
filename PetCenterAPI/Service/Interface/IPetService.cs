using PetCenterAPI.DTOs.Requests;
using static PetCenterAPI.DTOs.Requests.CustomerProfile.PetRequestDTO;

namespace PetCenterAPI.Service.Interface
{
    public interface IPetService
    {
        Task<List<ReadPetListDTO>> GetMyPetsAsync(Guid customerId);
        Task<ReadPetDetailDTO?> GetPetDetailsAsync(Guid petId, Guid customerId);
        Task<List<VetPetRequestDTO.ReadVetPetListDTO>> GetAllPetsForVetAsync();
        Task<VetPetRequestDTO.ReadVetPetDetailDTO?> GetPetDetailForVetAsync(Guid petId);
    }
}