using PetCenterClient.ViewModels.AI;

namespace PetCenterClient.Services.Interface
{
    public interface IAIClassifyAPIClient
    {
        Task<AIResultViewModel?> ClassifyAsync(IFormFile image);
    }
}
