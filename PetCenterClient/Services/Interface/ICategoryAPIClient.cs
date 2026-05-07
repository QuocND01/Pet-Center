using PetCenterClient.DTOs;

namespace PetCenterClient.Services.Interface
{
    public interface ICategoryAPIClient
    {
        Task<OdataResponse<ReadCategoryDTOs>> GetAllCategoryAsync(string? search, int page = 1);
        Task<ReadCategoryDTOs> DetailsCategoryAsync(Guid? id);
        Task AddCategoryAsync(CreateCategoryDTOs createCategory);
        Task UpdateCategoryAsync(Guid? id, UpdateCategoryDTOs updateCategory);
        Task DeleteCategoryAsync(Guid? id);
    }
}
