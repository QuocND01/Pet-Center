using PetCenterClient.Common;
using PetCenterClient.DTOs;

namespace PetCenterClient.Services.Interface
{
    public interface ICategoryAPIClient
    {
        Task<OdataResponse<ReadCategoryDTOForCustomer>> GetAllCategoryAsync(string? search, int page = 1);

        Task<PagedResponse<ReadCategoryDTO>> GetAllCategoryAdminAsync(
      string? search, bool? isActive, int page = 1, int pageSize = 10);
        Task<ReadCategoryDTO> DetailsCategoryAsync(Guid? id);
        Task AddCategoryAsync(CreateCategoryDTO createCategory);
        Task UpdateCategoryAsync(Guid? id, UpdateCategoryDTO updateCategory);
        Task ChangeCategoryStatusAsync(Guid id, Status status);
    }
}
