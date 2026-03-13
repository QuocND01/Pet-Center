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

        Task<bool> CheckCategoryExist(string categoryName);

        Task AddAttribute(CategoryAttribute attributeValue);
        Task DeleteAttributeByCategoryID(Guid id);
        Task<IEnumerable<CategoryAttribute>> GetAllCategoryAttributeByCategoryIDAsync(Guid id);
    }
}
