using PetCenterClient.ViewModels;

namespace PetCenterClient.Services.Interface
{
    public interface IDiseaseAPIClient
    {
        Task<List<ReadDiseaseViewModel>?> GetAllDiseasesAsync(string query = "");
        Task<ReadDiseaseViewModel?> GetDiseaseDetailsAsync(Guid id);
        Task<bool> AddDiseaseAsync(MutateDiseaseViewModel dto);
        Task<bool> UpdateDiseaseAsync(Guid id, MutateDiseaseViewModel dto);
        Task<bool> DeleteDiseaseAsync(Guid id);
    }
}