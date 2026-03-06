using PetCenterClient.DTOs;

namespace PetCenterClient.Services.Interface
{
    public interface ICategoryService
    {
        Task<IEnumerable<ReadCategoryDTOs>> GetAllCategoryAsync();
    }
}
