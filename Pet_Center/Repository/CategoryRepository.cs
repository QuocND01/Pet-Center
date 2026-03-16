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

        public async Task AddAttribute(CategoryAttribute attributeValue)
        {
            _db.CategoryAttributes.Add(attributeValue);
            await _db.SaveChangesAsync();
        }

        public async Task AddCategoryAsync(Category category)
        {
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> CheckCategoryExist(string categoryName)
        {
            return await _db.Categories.AnyAsync(c => c.CategoryName == categoryName && c.IsActive == true);
        }

        public async Task DeleteAttributeByCategoryID(Guid id)
        {
            var attributes = await _db.CategoryAttributes
             .Where(a => a.CategoryId == id && a.IsActive == true)
             .ToListAsync();

            foreach (var attr in attributes)
            {
                attr.IsActive = false;
            }

            await _db.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(Guid id)
        {
            Category c = _db.Categories.Find(id);
            c.IsActive = false;
            await _db.SaveChangesAsync();
        }

        public IQueryable<Category> GetAllCategory()
        {
            return _db.Categories
                .Where(c => c.IsActive == true)
                .Select(c => new Category
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    CategoryLogo = c.CategoryLogo,

                    CategoryAttributes = c.CategoryAttributes
                        .Where(a => a.IsActive == true)
                        .ToList()
                });
        }

        //public async Task<IEnumerable<Category>> GetAllCategoryAsync()
        //{
        //   return await _db.Categories.Where(c => c.IsActive == true).ToListAsync();
        //}

        public async Task<IEnumerable<CategoryAttribute>> GetAllCategoryAttributeByCategoryIDAsync(Guid id)
        {
            return await _db.CategoryAttributes.Where(c => c.CategoryId.Equals(id) && c.IsActive == true).ToListAsync();
        }

        public Task<Category?> GetCategoryByIdAsync(Guid id)
        {
            return _db.Categories
                .Where(c => c.IsActive == true)
                .Include(c => c.CategoryAttributes.Where(a => a.IsActive == true))
                .FirstOrDefaultAsync(x => x.CategoryId == id);
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            _db.Categories.Update(category);
            await _db.SaveChangesAsync();
        }
    }
}
