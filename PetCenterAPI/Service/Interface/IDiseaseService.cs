using static PetCenterAPI.DTOs.Requests.DiseaseDTO;

namespace PetCenterAPI.Service.Interface
{
    public interface IDiseaseService
    {
        IQueryable<ReadDiseaseDTO> GetAllDiseasesQuery();
        Task<ReadDiseaseDTO?> GetDiseaseByIdAsync(Guid id);
        Task<bool> AddDiseaseAsync(MutateDiseaseDTO dto);
        Task<bool> UpdateDiseaseAsync(Guid id, MutateDiseaseDTO dto);
        Task<bool> DeleteDiseaseAsync(Guid id);
    }
}