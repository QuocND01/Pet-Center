using PetCenterAPI.Models;

namespace PetCenterAPI.Repository.Interface
{
    public interface IDiseaseRepository
    {
        Task<Disease?> GetDiseaseByIdAsync(Guid id);
        Task AddDiseaseAsync(Disease disease);
        Task UpdateDiseaseAsync(Disease disease);
        Task SaveAsync();
    }
}