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


        public IQueryable<Product> GetAllProduct()
        {
            try
            {
                return _db.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Include(p => p.Images)
                .Include(p => p.ProductAttributes)
                    .ThenInclude(pa => pa.CategoryAttribute).Where(p => p.IsActive == true)
                .AsQueryable();
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
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


        public async Task<bool> CheckProductExist(string productName, Guid brandId, Guid categoryId)
        {
            return await _db.Products.AnyAsync(p =>
        p.ProductName == productName &&
        p.BrandId == brandId &&
        p.CategoryId == categoryId &&
        p.IsActive);
        }

    }
}
