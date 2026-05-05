using AutoMapper;
using AutoMapper.QueryableExtensions;
using Humanizer;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using ProductAPI.DTOs;
using ProductAPI.Models;
using ProductAPI.Repository.Interface;
using ProductAPI.Service.Interface;

namespace ProductAPI.Service
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly HttpClient _inventoryClient;
        private readonly HttpClient _orderClient;
        private readonly IMapper _mapper;
        public ProductService(IProductRepository productRepository, IMapper mapper, ICloudinaryService service, IHttpClientFactory httpClientFactory)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _cloudinaryService = service;
            _inventoryClient = httpClientFactory.CreateClient("InventoryAPI");
            _orderClient = httpClientFactory.CreateClient("OrdersAPI");
        }


        public async Task AddProductAsync(CreateProductDTO createProduct)
        {
            bool productHasExist = false;
            productHasExist = await _productRepository.CheckProductExist(createProduct.ProductName, createProduct.BrandId, createProduct.CategoryId);
            if (productHasExist)
            {
                throw new InvalidOperationException("Product already exists");
            }
            else
            {
                var product = _mapper.Map<Product>(createProduct);

                product.ProductId = Guid.NewGuid();
                product.AddedAt = DateTime.UtcNow;
                product.Images ??= new List<Image>();
                if (createProduct.ImageFiles != null && createProduct.ImageFiles.Any())
                {
                    foreach (var file in createProduct.ImageFiles)
                    {
                        // 1️⃣ Upload trước
                        var uploadResult = await _cloudinaryService
                            .UploadImageAsync(file, "products");

                        if (uploadResult == null ||
                            uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                        {
                            throw new Exception("Upload ảnh thất bại");
                        }

                        // 2️⃣ Tạo Image entity
                        var image = new Image
                        {
                            ImageId = Guid.NewGuid(),
                            ImageUrl = uploadResult.SecureUrl.ToString(),
                            PublicId = uploadResult.PublicId,
                            IsActive = true
                        };

                        // 3️⃣ Gán trực tiếp vào navigation property
                        product.Images.Add(image);
                    }
                }

                // 4️⃣ Save
                await _productRepository.AddProductAsync(product);
            }
        }



        public async Task DeleteProductAsync(Guid id)
        {
            await _productRepository.DeleteProductAsync(id);
        }

        public async Task<List<ReadProductDTO>> GetAllProductAsync(ODataQueryOptions<ReadProductDTO> queryOptions)
        {
            var query = _productRepository
                .GetAllProduct()
                .ProjectTo<ReadProductDTO>(_mapper.ConfigurationProvider);

            var filtered = (IQueryable<ReadProductDTO>)queryOptions.ApplyTo(query);

            var products = await filtered.ToListAsync();

            if (!products.Any())
                return products;

            var productIds = products.Select(p => p.ProductId).ToList();

            var stocks = await GetStocksFromInventory(productIds);

            var stockDict = stocks.GroupBy(x => x.ProductId).ToDictionary(g => g.Key, g => g.First());

            foreach (var p in products)
            {
                p.StockQuantity = stockDict.TryGetValue(p.ProductId, out var s)
                    ? s.QuantityAvailable
                    : 0;
            }

            return products;
        }

        private async Task<List<StockDto>> GetStocksFromInventory(List<Guid> productIds)
        {
            if (productIds == null || !productIds.Any())
                return new List<StockDto>();

            try
            {
                var response = await _inventoryClient.PostAsJsonAsync(
                    "api/Inventory/stocks", productIds);

                if (!response.IsSuccessStatusCode)
                {
                    return new List<StockDto>();
                }

                return await response.Content.ReadFromJsonAsync<List<StockDto>>()
                       ?? new List<StockDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<StockDto>();
            }
        }


        public async Task<ReadProductDTO> GetProductByIdAsync(Guid id)
        {
            var product = await _productRepository.GetProductByIdAsync(id);

            if (product == null)
                throw new KeyNotFoundException("Product not found");

            var result = _mapper.Map<ReadProductDTO>(product);

            var stocks = await GetStocksFromInventory(new List<Guid> { id });

            result.StockQuantity = stocks.FirstOrDefault()?.QuantityAvailable ?? 0;

            return result;
        }

        public async Task UpdateProductAsync(Guid id, UpdateProductDTO updateproduct)
        {
            var product = await _productRepository.GetProductByIdAsync(id);

            if (product == null)
                throw new KeyNotFoundException("Product not found");

            if (updateproduct.BrandId == null || updateproduct.CategoryId == null)
                throw new Exception("BrandId and CategoryId are required");
            bool productHasExist = await _productRepository.CheckProductExist(updateproduct.ProductName, updateproduct.BrandId.Value, updateproduct.CategoryId.Value);

            if (productHasExist &&
               (product.ProductName != updateproduct.ProductName ||
                product.BrandId != updateproduct.BrandId ||
                product.CategoryId != updateproduct.CategoryId))
            {
                throw new InvalidOperationException("Product already exists");
            }

            _mapper.Map(updateproduct, product);

            product.UpdateAt = DateTime.UtcNow;

            product.Images ??= new List<Image>();
            product.ProductAttributes ??= new List<ProductAttribute>();

            // 1️⃣ xử lý ảnh bị xoá
            var existingImages = updateproduct.ExistingImages ?? new List<string>();

            var currentImages = product.Images.ToList();

            foreach (var img in currentImages)
            {
                if (!existingImages.Any(x => string.Equals(x, img.ImageUrl, StringComparison.OrdinalIgnoreCase)))
                {
                    await _cloudinaryService.DeleteImageAsync(img.PublicId);

                    product.Images.Remove(img);
                }
            }

            // 2️⃣ upload ảnh mới
            if (updateproduct.ImageFiles != null && updateproduct.ImageFiles.Any())
            {
                foreach (var file in updateproduct.ImageFiles)
                {
                    var uploadResult = await _cloudinaryService
                        .UploadImageAsync(file, "products");

                    if (uploadResult == null ||
                        uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        throw new Exception("Upload ảnh thất bại");
                    }

                    product.Images.Add(new Image
                    {
                        ImageUrl = uploadResult.SecureUrl.ToString(),
                        PublicId = uploadResult.PublicId,
                        ProductId = product.ProductId,
                        IsActive = true
                    });
                }
            }

            // 2️⃣ UPDATE ATTRIBUTES
            if (updateproduct.Attributes != null)
            {
                var existingAttrs = product.ProductAttributes ??= new List<ProductAttribute>();

                foreach (var oldAttr in existingAttrs)
                {
                    if (!updateproduct.Attributes.Any(a =>
                        string.Equals(a.AttributeValue, oldAttr.AttributeValue, StringComparison.OrdinalIgnoreCase)))
                    {
                        oldAttr.IsActive = false;
                    }
                }

                foreach (var newAttr in updateproduct.Attributes)
                {
                    var match = existingAttrs.FirstOrDefault(a =>
                        string.Equals(a.AttributeValue, newAttr.AttributeValue, StringComparison.OrdinalIgnoreCase));

                    if (match == null)
                    {
                        existingAttrs.Add(new ProductAttribute
                        {
                            ProductId = product.ProductId,
                            AttributeValue = newAttr.AttributeValue,
                            IsActive = true
                        });
                    }
                    else
                    {
                        match.IsActive = true;
                    }
                }
            }
            await _productRepository.UpdateProductAsync(product);
        }

        public async Task<IEnumerable<ReadProductDTO>> GetNewProducts()
        {
            var products = await _productRepository.GetNewProduct();
            return _mapper.Map<List<ReadProductDTO>>(products);
        }

        public async Task<IEnumerable<ReadProductDTO>> GetHotProducts()
        {
            List<Guid>? productIds;

            try
            {
                productIds = await _orderClient.GetFromJsonAsync<List<Guid>>("api/OrderDetails/hot-products");
            }
            catch (Exception ex)
            {
                return new List<ReadProductDTO>();
            }

            if (productIds == null || !productIds.Any())
                return new List<ReadProductDTO>();

            var products = await _productRepository.GetProductsByIds(productIds);

            return _mapper.Map<List<ReadProductDTO>>(products);
        }

        public async Task<List<SelectProductDto>> GetProductSelectListAsync()
        {
            // Logic: Chỉ lấy sản phẩm chưa bị xóa và đang hoạt động
            return await _productRepository.GetActiveProductsAsync<SelectProductDto>(p => p.IsActive);
        }

        public async Task<List<SelectProductDto>> GetProductSelectListToViewAsync()
        {
            return await _productRepository.GetActiveProductsAsync<SelectProductDto>();
        }


        //public async Task IncreaseStockBulk(List<IncreaseStockItemDto> items)
        //{
        //    if (items == null || !items.Any()) return;

        //    // 🔥 gộp product trùng
        //    var grouped = items
        //        .GroupBy(x => x.ProductId)
        //        .Select(g => new
        //        {
        //            ProductId = g.Key,
        //            Quantity = g.Sum(x => x.Quantity)
        //        })
        //        .ToList();

        //    var ids = grouped.Select(x => x.ProductId).ToList();

        //    var products = await _productRepository.GetByIds(ids);

        //    foreach (var item in grouped)
        //    {
        //        var product = products.FirstOrDefault(x => x.ProductId == item.ProductId);

        //        if (product == null) continue;

        //        product.StockQuantity ??= 0;
        //        product.StockQuantity += item.Quantity;
        //        product.UpdateAt = DateTime.UtcNow;
        //    }

        //    await _productRepository.SaveChangesAsync();
        //}
        //public async Task DecreaseStockBulk(List<DecreaseStockItemDto> items)
        //{
        //    if (items == null || !items.Any()) return;

        //    // ❗ validate
        //    if (items.Any(x => x.Quantity <= 0))
        //        throw new Exception("Quantity must be greater than 0");

        //    // 🔥 gộp product trùng
        //    var grouped = items
        //        .GroupBy(x => x.ProductId)
        //        .Select(g => new
        //        {
        //            ProductId = g.Key,
        //            Quantity = g.Sum(x => x.Quantity)
        //        })
        //        .ToList();

        //    var ids = grouped.Select(x => x.ProductId).ToList();

        //    var products = await _productRepository.GetByIds(ids);

        //    // 🔥 tối ưu lookup
        //    var productDict = products.ToDictionary(x => x.ProductId);

        //    foreach (var item in grouped)
        //    {
        //        if (!productDict.TryGetValue(item.ProductId, out var product))
        //            throw new Exception($"Product {item.ProductId} not found");

        //        product.StockQuantity ??= 0;

        //        // ❗ check đủ hàng
        //        if (product.StockQuantity < item.Quantity)
        //            throw new Exception($"Product {item.ProductId} is out of stock");

        //        product.StockQuantity -= item.Quantity;
        //        product.UpdateAt = DateTime.UtcNow;
        //    }

        //    await _productRepository.SaveChangesAsync();
        //}

        //public async Task<bool> DecreaseStockAsync(Guid productId, int quantity)
        //{
        //    return await _productRepository.DecreaseStockAsync(productId, quantity);
        //}

        //public async Task<bool> IncreaseStockAsync(Guid productId, int quantity)
        //{
        //    // Chỗ này sau này bạn có thể cắm thêm logic ghi log: "Sản phẩm A được cộng lại X cái do đơn hàng Y bị hủy"
        //    return await _productRepository.IncreaseStockAsync(productId, quantity);
        //}
        //public async Task<ReadProductDTO> GetProductByIdIncludeDeletedAsync(Guid id)
        //{
        //    var product = await _productRepository.GetProductByIdIncludeDeletedAsync(id);
        //    return _mapper.Map<ReadProductDTO>(product);
        //}

        //Code Hồ mới thêm
        public async Task<ProductInternalDto?> GetInternalAsync(Guid productId)
        {
            var product = await _productRepository.GetByIdInternalAsync(productId);
            if (product == null) return null;
            return new ProductInternalDto
            {
                ProductName = product.ProductName,
                ImageUrl = product.Images.FirstOrDefault()?.ImageUrl
            };
        }
    }
}
