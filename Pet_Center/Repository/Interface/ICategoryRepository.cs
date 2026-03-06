using ProductAPI.Models;

namespace ProductAPI.Repository.Interface
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllCategoryAsync();
        Task<IEnumerable<CategoryAttribute>> GetAllCategoryAttributeByCategoryIDAsync(Guid id);
    }
}
