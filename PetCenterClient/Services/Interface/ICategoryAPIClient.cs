using PetCenterClient.Common;
using PetCenterClient.DTOs;

namespace PetCenterClient.Services.Interface
{
    public interface ICategoryAPIClient
    {
        Task<OdataResponse<ReadCategoryViewModelForCustomer>> GetAllCategoryAsync();

        Task<PagedResponse<ReadCategoryViewModel>> GetAllCategoryAdminAsync(
      string? search, bool? isActive, int page = 1, int pageSize = 10);
        Task<ReadCategoryViewModel> DetailsCategoryAsync(Guid? id);
        Task AddCategoryAsync(CreateCategoryViewModel createCategory);
        Task UpdateCategoryAsync(Guid? id, UpdateCategoryViewModel updateCategory);
        Task ChangeCategoryStatusAsync(Guid id, Status status);
    }
}
