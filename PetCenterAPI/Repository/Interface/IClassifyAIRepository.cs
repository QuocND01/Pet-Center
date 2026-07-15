using PetCenterAPI.DTOs.Requests.AI;

namespace PetCenterAPI.Repository.Interface
{
    public interface IClassifyAIRepository
    {
        Task<AIRequestDTO?> PredictAsync(IFormFile image);
    }
}
