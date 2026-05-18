using ProductAPI.Common;
using ProductAPI.DTOs;
using ProductAPI.Models;

namespace ProductAPI.Service.Interface
{
    public interface ICategoryService
    {
        IQueryable<ReadCategoryDTOForCustomer> GetAllCategory();

        Task<PagedResult<ReadCategoryDTO>> GetAllCategoryAdminAsync(
    CategorySpecification spec);

        Task<ReadCategoryDTO?> GetCategoryByIdAsync(Guid id);
        Task AddCategoryAsync(CreateCategoryDTO category);
        Task UpdateCategoryAsync(Guid id, UpdateCategoryDTO category);
        Task ChangeCategoryStatusAsync(
       Guid id,
       Status status);
        Task<IEnumerable<ReadCategoryAttributeDTOs>> GetAllCategoryAttributeByCategoryIDAsync(Guid id);
    }
}
