using PetCenterClient.ViewModels.AI;

namespace PetCenterClient.Services.Interface
{
    public interface IAIClassifyService
    {
        Task<AIResultViewModel?> ClassifyAsync(IFormFile image);
    }
}
