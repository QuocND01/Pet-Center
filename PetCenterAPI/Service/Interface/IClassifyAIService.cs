using PetCenterAPI.DTOs.Requests.AI;

namespace PetCenterAPI.Service.Interface
{
    public interface IClassifyAIService
    {
        Task<AIResultDTO?> PredictAsync(IFormFile image);
    }
}
