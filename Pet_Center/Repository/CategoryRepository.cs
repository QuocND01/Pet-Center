using Microsoft.EntityFrameworkCore;
using ProductAPI.Models;
using ProductAPI.Repository.Interface;

namespace ProductAPI.Repository
{
    public class CategoryRepository : ICategoryRepository
    {
        private PetCenterContext _db;

        public CategoryRepository(PetCenterContext petCenterContext)
        {
            _db = petCenterContext;
        }
        public async Task<IEnumerable<Category>> GetAllCategoryAsync()
        {
           return await _db.Categories.Where(c => c.IsActive == true).ToListAsync();
        }

        public async Task<IEnumerable<CategoryAttribute>> GetAllCategoryAttributeByCategoryIDAsync(Guid id)
        {
            return await _db.CategoryAttributes.Where(c => c.CategoryId.Equals(id)).ToListAsync();
        }
    }
}
