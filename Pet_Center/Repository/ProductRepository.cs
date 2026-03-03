using Microsoft.EntityFrameworkCore;
using ProductAPI.Models;
using ProductAPI.Repository.Interface;

namespace ProductAPI.Repository
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
             p.IsActive = false;
            await _db.SaveChangesAsync();
        }


        public async Task DeleteProductAttributesByProductIdAsync(Guid productId)
        {
            var attributes = await _db.ProductAttributes
                .Where(x => x.ProductId == productId)
                .ToListAsync();

            if (attributes.Any())
            {
                _db.ProductAttributes.RemoveRange(attributes);
                await _db.SaveChangesAsync();
            }
        }


        public async Task<IEnumerable<Product>> GetAllProductAsync()
        {
            return await _db.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Include(p => p.Images)
                .Include(p => p.ProductAttributes)
                    .ThenInclude(pa => pa.CategoryAttribute).Where(p => p.IsActive == true)
                .ToListAsync();
        }

        public Task<Product?> GetProductByIdAsync(Guid id)
        {
            return _db.Products.Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Include(p => p.Images)
                .Include(p => p.ProductAttributes)
                    .ThenInclude(pa => pa.CategoryAttribute).Where(p => p.IsActive == true)
                .FirstOrDefaultAsync(x => x.ProductId == id);
        }

   

        public async Task UpdateProductAsync(Product product)
        {
            _db.Products.Update(product);
            await _db.SaveChangesAsync();
        }
    }
}
