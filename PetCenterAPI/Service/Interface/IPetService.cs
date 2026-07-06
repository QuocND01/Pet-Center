using PetCenterAPI.DTOs.Requests;
using static PetCenterAPI.DTOs.Requests.CustomerProfile.PetRequestDTO;

namespace PetCenterAPI.Service.Interface
{
    public interface IPetService
    {
        IQueryable<ReadPetListDTO> GetMyPetsQuery(Guid customerId);
        Task<ReadPetDetailDTO?> GetPetDetailsAsync(Guid petId, Guid customerId);

        IQueryable<VetPetRequestDTO.ReadVetPetListDTO> GetAllPetsForVetQuery();
        Task<VetPetRequestDTO.ReadVetPetDetailDTO?> GetPetDetailForVetAsync(Guid petId);

        Task<bool> AddPetAsync(Guid customerId, MutatePetDTO dto);
        Task<bool> UpdatePetAsync(Guid petId, Guid customerId, MutatePetDTO dto, bool isVet);
        Task<bool> DeletePetAsync(Guid petId, Guid customerId, bool isVet);
    }
}