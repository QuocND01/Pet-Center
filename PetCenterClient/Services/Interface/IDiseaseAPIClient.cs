using PetCenterClient.ViewModels;

namespace PetCenterClient.Services.Interface
{
    public interface IDiseaseAPIClient
    {
        Task<List<ReadDiseaseViewModel>?> GetAllDiseasesAsync(string query = "");
        Task<ReadDiseaseViewModel?> GetDiseaseDetailsAsync(Guid id);
        Task<(bool success, string message)> AddDiseaseAsync(MutateDiseaseViewModel dto);
        Task<(bool success, string message)> UpdateDiseaseAsync(Guid id, MutateDiseaseViewModel dto);
        Task<(bool success, string message)> DeleteDiseaseAsync(Guid id);
    }
}