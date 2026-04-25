using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using ProductAPI.Models;
using ProductAPI.Repository.Interface;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;

namespace ProductAPI.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly PetCenterContext _db;
        private readonly IMapper _mapper;
        public ProductRepository(PetCenterContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
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
                .Include(p => p.Images)
                .Include(p => p.ProductAttributes)
                    .ThenInclude(pa => pa.CategoryAttribute).Where(p => p.IsActive == true)
                .AsQueryable();
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<Product>> GetNewProduct()
        {
            var threeMonthsAgo = DateTime.Now.AddMonths(-3);

            return await _db.Products
                .Where(p => p.IsActive && p.AddedAt >= threeMonthsAgo)
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.ProductAttributes)
                    .ThenInclude(pa => pa.CategoryAttribute)
                .ToListAsync();
        }


        public async Task<IEnumerable<Product?>> GetProductsByIds(List<Guid> ids)
        {
            return await _db.Products
                .Where(p => p.IsActive && ids.Contains(p.ProductId))
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.ProductAttributes)
                    .ThenInclude(pa => pa.CategoryAttribute)
                .ToListAsync();
        }



        public Task<Product?> GetProductByIdAsync(Guid id)
        {
            return _db.Products.Include(p => p.Brand)
                .Include(p => p.Category)
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

        public async Task<List<T>> GetActiveProductsAsync<T>(Expression<Func<Product, bool>>? filter = null)
        {
            IQueryable<Product> query = _db.Products.AsNoTracking();

            if (filter != null)
                query = query.Where(filter);

            // Sử dụng ProjectTo để SQL chỉ chọn đúng các cột có trong DTO
            return await query.ProjectTo<T>(_mapper.ConfigurationProvider)
                              .ToListAsync();
        }
        public async Task<List<Product>> GetByIds(List<Guid> ids)
        {
            return await _db.Products
                .Where(x => ids.Contains(x.ProductId))
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        //public async Task<bool> DecreaseStockAsync(Guid productId, int quantity)
        //{
        //    // Tìm sản phẩm
        //    var product = await _db.Products.FindAsync(productId);

        //    // Kiểm tra xem sản phẩm có tồn tại và kho có đủ hàng không
        //    if (product == null)
        //    {
        //        return false;
        //    }

        //    // Trừ kho và lưu lại
        //    product.StockQuantity -= quantity;
        //    _db.Products.Update(product);
        //    await _db.SaveChangesAsync();

        //    return true;
        //}
        //public async Task<bool> IncreaseStockAsync(Guid productId, int quantity)
        //{
        //    var product = await _db.Products.FindAsync(productId);

        //    if (product == null)
        //    {
        //        return false;
        //    }

        //    // Cộng trả lại kho
        //    product.StockQuantity += quantity;

        //    _db.Products.Update(product);
        //    await _db.SaveChangesAsync();

        //    return true;
        //}

        //public Task<Product?> GetProductByIdIncludeDeletedAsync(Guid id)
        //{
        //    return _db.Products
        //        .Include(p => p.Brand)
        //        .Include(p => p.Category)
        //        .Include(p => p.Images)
        //        .Include(p => p.ProductAttributes)
        //            .ThenInclude(pa => pa.CategoryAttribute)
        //        // Bỏ filter IsActive để lấy được cả sản phẩm đã xóa mềm
        //        .FirstOrDefaultAsync(x => x.ProductId == id);
        //}
    }
}
