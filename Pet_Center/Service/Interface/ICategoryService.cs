using ProductAPI.DTOs;
using ProductAPI.Models;

namespace ProductAPI.Service.Interface
{
    public interface ICategoryService
    {
        Task<IEnumerable<ReadCategoryDTOs>> GetAllCategoryAsync();
        Task<IEnumerable<CategoryAttribute>> GetAllCategoryAttributeByCategoryIDAsync(Guid id);
    }
}
