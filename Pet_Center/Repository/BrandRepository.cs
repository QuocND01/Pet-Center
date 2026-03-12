using Microsoft.EntityFrameworkCore;
using ProductAPI.Models;
using ProductAPI.Repository.Interface;

namespace ProductAPI.Repository
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

        public async Task<bool> CheckBrandExist(string brandName)
        {
            return await _db.Brands.AnyAsync(b => b.BrandName == brandName && b.IsActive == true);
        }

        public async Task DeleteBrandAsync(Guid id)
        {
            Brand p = _db.Brands.Find(id);
            p.IsActive = false;
            await _db.SaveChangesAsync();
        }

        public IQueryable<Brand> GetAllBrand()
        {
            try
            {
                return _db.Brands.Where(b => b.IsActive == true).AsQueryable();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        //public async Task<IEnumerable<Brand>> GetAllBrandAsync()
        //{
        //    return await _db.Brands.Where(b => b.IsActive == true).ToListAsync();
        //}

        public Task<Brand?> GetBrandByIdAsync(Guid id)
        {
            return _db.Brands.Where(p => p.IsActive == true)
                .FirstOrDefaultAsync(x => x.BrandId == id);
        }

        public async Task UpdateBrandAsync(Brand brand)
        {
            _db.Brands.Update(brand);
            await _db.SaveChangesAsync();
        }
    }
}
