using Microsoft.EntityFrameworkCore;
using PetCenterAPI.Common;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;

namespace PetCenterAPI.Repository
{
    public class CategoryRepository : ICategoryRepository
    {
        private PetCenterContext _db;

        public CategoryRepository(PetCenterContext petCenterContext)
        {
            _db = petCenterContext;
        }


        public async Task AddCategoryAsync(Category category)
        {
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> CheckCategoryExistAsync(
    string categoryName,
    Guid? excludeId = null)
        {
            return await _db.Categories.Where(c => c.Status != Status.Deleted).AnyAsync(c =>
                c.CategoryName == categoryName.Trim() &&
                c.Status == Status.Active &&
                (!excludeId.HasValue || c.CategoryId != excludeId.Value));
        }

        public async Task ChangeCategoryStatusAsync(
      Guid id,
      Status status)
        {
            await _db.Categories
                .Where(c => c.CategoryId == id)
                .ExecuteUpdateAsync(s =>
                    s.SetProperty(c => c.Status, status));

            // inactive/deleted => disable attributes
            if (status == Status.Deleted)
            {
                await _db.CategoryAttributes
                    .Where(a => a.CategoryId == id)
                    .ExecuteUpdateAsync(s =>
                        s.SetProperty(a => a.IsActive, false));
            }
        }

        public IQueryable<Category> GetAllCategory()
        {
            return _db.Categories
                .Where(c => c.Status == Status.Active)
                .Select(c => new Category
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    CategoryLogo = c.CategoryLogo,
                    CategoryDescription = c.CategoryDescription,
                    CategoryAttributes = c.CategoryAttributes
                        .Where(a => a.IsActive)
                        .ToList()
                });
        }


        public async Task<(IEnumerable<Category> Items, int Total)> GetAllCategoryAdminAsync(
    CategorySpecification spec)
        {
            var query = _db.Categories.Where(c => c.Status != Status.Deleted)
                .Select(c => new Category
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    CategoryLogo = c.CategoryLogo,
                    CategoryDescription = c.CategoryDescription,
                    Status = c.Status,
                    CategoryAttributes = c.CategoryAttributes
                        .Where(a => a.IsActive)
                        .ToList()
                })
                .Where(spec.ToExpression());

            var total = await query.CountAsync();
            var items = await query
                .Skip((spec.Page - 1) * spec.PageSize)
                .Take(spec.PageSize)
                .ToListAsync();

            return (items, total);
        }


        public async Task<IEnumerable<CategoryAttribute>> GetAllCategoryAttributeByCategoryIDAsync(Guid id)
        {
            return await _db.CategoryAttributes.Where(c => c.CategoryId.Equals(id) && c.IsActive == true).ToListAsync();
        }

        public Task<Category?> GetCategoryByIdAsync(Guid id)
        {
            return _db.Categories
                .Include(c => c.CategoryAttributes.Where(a => a.IsActive == true))
                .FirstOrDefaultAsync(x => x.CategoryId == id);
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            _db.Categories.Update(category);
            foreach (var e in _db.ChangeTracker.Entries<CategoryAttribute>())
            {
                Console.WriteLine(
                    $"{e.Entity.AttributeName} - {e.Entity.CategoryAttributeId} - {e.State}");
            }
            await _db.SaveChangesAsync();
        }
    }
}
