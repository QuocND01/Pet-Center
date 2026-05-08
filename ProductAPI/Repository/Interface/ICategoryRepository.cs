using ProductAPI.Models;

namespace ProductAPI.Repository.Interface
{
    public interface ICategoryRepository
    {
       // Task<IEnumerable<Category>> GetAllCategoryAsync();

        IQueryable<Category> GetAllCategory();

        Task<Category?> GetCategoryByIdAsync(Guid id);
        Task AddCategoryAsync(Category category);
        Task UpdateCategoryAsync(Category category);
        Task DeleteCategoryAsync(Guid id);

        Task<bool> CheckCategoryExistAsync(string categoryName);

        Task<IEnumerable<CategoryAttribute>> GetAllCategoryAttributeByCategoryIDAsync(Guid id);
    }
}
