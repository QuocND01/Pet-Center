using Microsoft.EntityFrameworkCore;
using Pet_Center.Models;
using Pet_Center.Repository.Interface;

namespace Pet_Center.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly PetCenterContext _db;

        public ProductRepository(PetCenterContext db)
        {
            _db = db;
        }


        public async Task AddProductAsync(Product product)
        {
            _db.Products.Add(product);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteProductAsync(Guid id)
        {
            Product p = _db.Products.Find(id);
            _db.Products.Remove(p);
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<Product>> GetAllProductAsync()
        {
            return await _db.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Include(p => p.Images)
                .Include(p => p.ProductAttributes)
                    .ThenInclude(pa => pa.CategoryAttribute)
                .ToListAsync();
        }

        public Task<Product?> GetProductByIdAsync(Guid id)
        {
            return _db.Products.Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Include(p => p.Images)
                .Include(p => p.ProductAttributes)
                    .ThenInclude(pa => pa.CategoryAttribute)
                .FirstOrDefaultAsync(x => x.ProductId == id);
        }

        public async Task UpdateProductAsync(Product product)
        {
            _db.Products.Update(product);
            await _db.SaveChangesAsync();
        }
    }
}
