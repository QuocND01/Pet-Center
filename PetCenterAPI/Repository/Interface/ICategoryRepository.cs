using PetCenterAPI.Common;
using PetCenterAPI.Models;

namespace PetCenterAPI.Repository.Interface
{
    public interface ICategoryRepository
    {
       // Task<IEnumerable<Category>> GetAllCategoryAsync();

        IQueryable<Category> GetAllCategory();

        Task<(IEnumerable<Category> Items, int Total)> GetAllCategoryAdminAsync(
      CategorySpecification spec);

        Task<Category?> GetCategoryByIdAsync(Guid id);
        Task AddCategoryAsync(Category category);
        Task UpdateCategoryAsync(Category category);
        Task ChangeCategoryStatusAsync(
       Guid id,
       Status status);

        Task<bool> CheckCategoryExistAsync(string categoryName);

        Task<IEnumerable<CategoryAttribute>> GetAllCategoryAttributeByCategoryIDAsync(Guid id);
    }
}
