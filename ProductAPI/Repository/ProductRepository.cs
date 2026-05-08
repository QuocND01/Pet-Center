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

        public async Task<IEnumerable<Product>> GetNewProductAsync()
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


        public async Task<IEnumerable<Product?>> GetProductsByIdsAsync(List<Guid> ids)
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


        public async Task<bool> CheckProductExistAsync(string productName, Guid brandId, Guid categoryId)
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

        // Hàm sử dụng lấy hình ảnh và tên sản phẩm của Hồ mới thêm
        public async Task<Product?> GetByIdInternalAsync(Guid productId)
    => await _db.Products
        .Include(p => p.Images.Where(i => i.IsActive))
        .AsNoTracking()
        .FirstOrDefaultAsync(p => p.ProductId == productId && p.IsActive);

        public async Task<List<Product>> GetProductsForSnapshotAsync(List<Guid> productIds)
        {
            return await _db.Products
                .AsNoTracking()
                .Include(x => x.Brand)
                .Include(x => x.Category)
                .Where(x => productIds.Contains(x.ProductId))
                .ToListAsync();
        }
    }
}
