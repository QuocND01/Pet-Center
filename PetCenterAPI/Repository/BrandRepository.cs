using Microsoft.EntityFrameworkCore;
using PetCenterAPI.Common;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;

namespace PetCenterAPI.Repository
{
    public class BrandRepository : IBrandRepository
    {
        private readonly PetCenterContext _db;

        public BrandRepository(PetCenterContext db)
        {
            _db = db;
        }

        public async Task AddBrandAsync(Brand brand)
        {
             _db.Brands.Add(brand);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> CheckBrandExistAsync(string brandName)
        {
            return await _db.Brands.AnyAsync(b => b.BrandName == brandName && b.Status == Status.Active);
        }

        public async Task ChangeBrandStatusAsync(
     Guid id,
     Status status)
        {
            await _db.Brands
                .Where(b => b.BrandId == id)
                .ExecuteUpdateAsync(s =>
                    s.SetProperty(b => b.Status, status));
        }

        public IQueryable<Brand> GetAllBrand()
        {
            try
            {
                return _db.Brands.Where(b => b.Status == Status.Active).AsQueryable();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<(IEnumerable<Brand> Items, int Total)> GetAllBrandAdminAsync(
     BrandSpecification spec)
        {
            var query = _db.Brands.Where(spec.ToExpression());

            var total = await query.CountAsync();
            var items = await query
                .Skip((spec.Page - 1) * spec.PageSize)
                .Take(spec.PageSize)
                .ToListAsync();

            return (items, total);
        }

        public Task<Brand?> GetBrandByIdAsync(Guid id)
        {
            return _db.Brands.FirstOrDefaultAsync(x => x.BrandId == id);
        }

        public async Task UpdateBrandAsync(Brand brand)
        {
            _db.Brands.Update(brand);
            await _db.SaveChangesAsync();
        }
    }
}
