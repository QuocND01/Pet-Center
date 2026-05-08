using ProductAPI.DTOs;
using ProductAPI.Models;

namespace ProductAPI.Service.Interface
{
    public interface ICategoryService
    {
        IQueryable<ReadCategoryDTOs> GetAllCategory();

        Task<ReadCategoryDTOs?> GetCategoryByIdAsync(Guid id);
        Task AddCategoryAsync(CreateCategoryDTOs category);
        Task UpdateCategoryAsync(Guid id, UpdateCategoryDTOs category);
        Task DeleteCategoryAsync(Guid id);
        Task<IEnumerable<ReadCategoryAttributeDTOs>> GetAllCategoryAttributeByCategoryIDAsync(Guid id);
    }
}
