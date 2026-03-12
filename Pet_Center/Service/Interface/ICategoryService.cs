using ProductAPI.DTOs;
using ProductAPI.Models;

namespace ProductAPI.Service.Interface
{
    public interface ICategoryService
    {
       // Task<IEnumerable<ReadCategoryDTOs>> GetAllCategoryAsync();
        IQueryable<ReadCategoryDTOs> GetAllCategory();

        Task<ReadCategoryDTOs?> GetCategoryByIdAsync(Guid id);
        Task AddCategoryAsync(CreateCategoryDTOs category);
        Task UpdateCategoryAsync(Guid id, UpdateCategoryDTOs category);
        Task DeleteCategoryAsync(Guid id);

        Task AddAttribute(CreateCategoryAttributeDTOs attributeValue);
        Task<IEnumerable<ReadCategoryAttributeDTOs>> GetAllCategoryAttributeByCategoryIDAsync(Guid id);
    }
}
