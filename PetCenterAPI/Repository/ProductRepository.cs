using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using PetCenterAPI.Common;
using PetCenterAPI.Helpers;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using static PetCenterAPI.DTOs.Requests.Product.ProductAttributeRequestDTO;
using static PetCenterAPI.DTOs.Responses.Product.ProductAttributeResponseDTO;

namespace PetCenterAPI.Repository
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
            //Debug
            Console.WriteLine($"Attributes Count: {product.ProductAttributes?.Count}");

            if (product.ProductAttributes != null)
            {
                foreach (var item in product.ProductAttributes)
                {
                    Console.WriteLine($"Value = {item.AttributeValue}");
                }
            }
            //Add inventory + gen SKU
            var brand = await _db.Brands
        .FirstAsync(x => x.BrandId == product.BrandId);

            var category = await _db.Categories
                .FirstAsync(x => x.CategoryId == product.CategoryId);

            product.Brand = brand;
            product.Category = category;

            var inventory = new Inventory
            {
                InventoryId = Guid.NewGuid(),
                ProductId = product.ProductId,
                SKU = SkuGenerator.Generate(product),
                QuantityAvailable = 0,
                LastUpdated = DateTime.UtcNow
            };

            product.Inventory = inventory;
            //Add product
            _db.Products.Add(product);
            await _db.SaveChangesAsync();
        }

        public async Task ChangeProductStatusAsync(
     Guid id,
     Status status,
     bool hardDeleteImages = false)
        {
            var product = await _db.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(x => x.ProductId == id);

            if (product == null)
                throw new Exception("Product not found");

            await _db.Products
                .Where(p => p.ProductId == id)
                .ExecuteUpdateAsync(s =>
                    s.SetProperty(p => p.Status, status));

            if (status == Status.Deleted)
            {
                var imageIds = product.ProductImages
                    .Select(i => i.ImageId)
                    .ToList();

                if (!imageIds.Any())
                    return;

                if (hardDeleteImages)
                {
                    await _db.ProductImages
                        .Where(i => imageIds.Contains(i.ImageId))
                        .ExecuteDeleteAsync();
                }
            }
        }

        public IQueryable<Product> GetAllProduct()
        {
            try
            {
                return _db.Products.Where(p => p.Status == Status.Active)
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.ProductImages.Where(i => i.IsActive == true))
                .Include(p => p.Inventory)
                .Include(p => p.ProductAttributes)
                    .ThenInclude(pa => pa.CategoryAttribute).Where(p => p.Status == Status.Active)
                    
                .AsQueryable();
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public async Task<(IEnumerable<Product> Items, int Total)> GetAllProductAdminAsync(
      ProductSpecification spec)
        {
            var query = _db.Products
                .Where(p => p.Status != Status.Deleted)
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.ProductImages.Where(i => i.IsActive == true))
                .Include(p => p.Inventory)
                .Include(p => p.ProductAttributes)
                    .ThenInclude(pa => pa.CategoryAttribute)
                .Where(spec.ToExpression());

            // Sorting
            if (!string.IsNullOrWhiteSpace(spec.SortBy))
            {
                switch (spec.SortBy.ToLower())
                {
                    case "productname":
                        query = spec.SortOrder.ToLower() == "desc"
                            ? query.OrderByDescending(x => x.ProductName)
                            : query.OrderBy(x => x.ProductName);
                        break;

                    case "productprice":
                        query = spec.SortOrder.ToLower() == "desc"
                            ? query.OrderByDescending(x => x.ProductPrice)
                            : query.OrderBy(x => x.ProductPrice);
                        break;

                    case "addedat":
                        query = spec.SortOrder.ToLower() == "desc"
                            ? query.OrderByDescending(x => x.AddedAt)
                            : query.OrderBy(x => x.AddedAt);
                        break;

                    case "updatedat":
                        query = spec.SortOrder.ToLower() == "desc"
                            ? query.OrderByDescending(x => x.UpdateAt)
                            : query.OrderBy(x => x.UpdateAt);
                        break;

                    default:
                        query = query.OrderByDescending(x => x.AddedAt);
                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(x => x.AddedAt);
            }

            var total = await query.CountAsync();

            var items = await query
                .Skip((spec.Page - 1) * spec.PageSize)
                .Take(spec.PageSize)
                .ToListAsync();

            return (items, total);
        }


        public async Task<bool> IsProductInOrderAsync(Guid productId)
        {
            return await _db.OrderDetails
                .AnyAsync(od =>
                    od.ProductId == productId &&
                    od.Order.Status != 4);
        }

        public async Task<IEnumerable<Product>> GetNewProductAsync()
        {
            var threeMonthsAgo = DateTime.Now.AddMonths(-3);

            return await _db.Products
             .Where(p => p.Status == Status.Active)
             .Where(p => p.AddedAt >= threeMonthsAgo)
             .OrderByDescending(p => p.AddedAt)   // Mới nhất trước
             .Take(16)                            // Top 20 sản phẩm mới
             .Include(p => p.Brand)
             .Include(p => p.Category)
             .Include(p => p.ProductImages.Where(i => i.IsActive== true))
             .Include(p => p.Inventory)
             .Include(p => p.ProductAttributes)
                 .ThenInclude(pa => pa.CategoryAttribute)
             .ToListAsync();
        }


        public async Task<IEnumerable<Product>> GetHotProduct()
        {
            var threeMonthsAgo = DateTime.Now.AddMonths(-3);

            var productIds = await _db.OrderDetails
               .Where(od => od.Order.OrderDate >= threeMonthsAgo && od.Order.Status == 4)
                .GroupBy(od => od.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalSold = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(16)
                .Select(x => x.ProductId)
                .ToListAsync();

            var products = await _db.Products
                .Where(p => p.Status == Status.Active)
                .Where(p => productIds.Contains(p.ProductId))
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.ProductImages.Where(i => i.IsActive == true))
                .Include(p => p.Inventory)
                .Include(p => p.ProductAttributes)
                    .ThenInclude(pa => pa.CategoryAttribute)
                .ToListAsync();

            return products
                .OrderBy(p => productIds.IndexOf(p.ProductId))
                .ToList();
        }


        public Task<Product?> GetProductByIdAsync(Guid id)
        {
            return _db.Products.Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.ProductImages.Where(i => i.IsActive == true))
                .Include(p => p.Inventory)
                .Include(p => p.ProductAttributes)
                    .ThenInclude(pa => pa.CategoryAttribute)
                .FirstOrDefaultAsync(x => x.ProductId == id);
        }



        public async Task UpdateProductAsync(Product product)
        {
            _db.Products.Update(product);
            await _db.SaveChangesAsync();
        }


        public async Task<bool> CheckProductExistAsync(
        string productName,
        Guid brandId,
        Guid categoryId,
        List<ProductAttributeCompareDTO> attributes,
        Guid? excludeId = null)
        {
            var inputSet = attributes
                .Select(x => $"{x.CategoryAttributeId}|{x.AttributeValue.Trim().ToLower()}")
                .ToHashSet();

            var products = await _db.Products
                .Where(p =>
                    p.Status != Status.Deleted &&
                    p.ProductName == productName &&
                    p.BrandId == brandId &&
                    p.CategoryId == categoryId &&
                    (!excludeId.HasValue || p.ProductId != excludeId.Value))
                .Include(p => p.ProductAttributes)
                .ToListAsync();

            return products.Any(product =>
            {
                var dbSet = product.ProductAttributes
                    .Where(x => x.IsActive == true)
                    .Select(x => $"{x.CategoryAttributeId}|{x.AttributeValue.Trim().ToLower()}")
                    .ToHashSet();

                return dbSet.SetEquals(inputSet);
            });
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
        .Include(p => p.ProductImages.Where(i => i.IsActive == true))
        .AsNoTracking()
        .FirstOrDefaultAsync(p => p.ProductId == productId && p.Status == Status.Active);

        public async Task<List<Product>> GetProductsForSnapshotAsync(List<Guid> productIds)
        {
            return await _db.Products
                .AsNoTracking()
                .Include(x => x.Brand)
                .Include(x => x.Category)
                .Where(x => productIds.Contains(x.ProductId))
                .ToListAsync();
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
