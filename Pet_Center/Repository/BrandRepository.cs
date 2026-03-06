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

        public async Task<IEnumerable<Brand>> GetAllBrandAsync()
        {
            return await _db.Brands.Where(b => b.IsActive == true).ToListAsync();
        }
    }
}
