using AutoMapper;
using AutoMapper.QueryableExtensions;
using Humanizer;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using PetCenterAPI.Common;
using PetCenterAPI.DTOs;
using PetCenterAPI.Models;
using PetCenterAPI.Repository;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service.Interface;

namespace PetCenterAPI.Service
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly HttpClient _inventoryClient;
        private readonly HttpClient _orderClient;
        private readonly HttpClient _importClient;
        private readonly IMapper _mapper;
        public ProductService(IProductRepository productRepository, IMapper mapper, ICloudinaryService service, IHttpClientFactory httpClientFactory)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _cloudinaryService = service;
            _inventoryClient = httpClientFactory.CreateClient("InventoryAPI");
            _orderClient = httpClientFactory.CreateClient("OrdersAPI");
            _importClient = httpClientFactory.CreateClient("ImportStockAPI");
        }


        public async Task AddProductAsync(CreateProductDTO createProduct)
        {
            bool productHasExist = false;
            productHasExist = await _productRepository.CheckProductExistAsync(createProduct.ProductName, createProduct.BrandId, createProduct.CategoryId);
            if (productHasExist)
            {
                throw new InvalidOperationException("Product already exists");
            }
            else
            {
                var product = _mapper.Map<Product>(createProduct);

                product.ProductId = Guid.NewGuid();
                product.AddedAt = DateTime.UtcNow;
                product.ProductImages ??= new List<ProductImage>();
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
                        var image = new ProductImage
                        {
                            ImageId = Guid.NewGuid(),
                            ImageUrl = uploadResult.SecureUrl.ToString(),
                            PublicId = uploadResult.PublicId,
                            IsActive = true
                        };

                        // 3️⃣ Gán trực tiếp vào navigation property
                        product.ProductImages.Add(image);
                    }
                }

                // 4️⃣ Save
                await _productRepository.AddProductAsync(product);
            }
        }



        public async Task ChangeProductStatusAsync(Guid id, Status status)
        {
            var product = await _productRepository.GetProductByIdAsync(id);

            if (product == null)
                throw new Exception("Product not found");

            switch (status)
            {
                case Status.Active:
                case Status.Inactive:
                    {
                        await _productRepository.ChangeProductStatusAsync(id, status);
                        break;
                    }

                case Status.Deleted:
                    {
                        bool hasOrder = await HasProductInOrdersAsync(id);
                        bool hasImport = await HasProductInImportsAsync(id);

                        bool canHardDelete = !hasOrder && !hasImport;

                        if (canHardDelete)
                        {
                            foreach (var image in product.ProductImages)
                            {
                                if (!string.IsNullOrEmpty(image.PublicId))
                                {
                                    try
                                    {
                                        await _cloudinaryService.DeleteImageAsync(image.PublicId);
                                    }
                                    catch
                                    {
                                    }
                                }
                            }

                            await _productRepository.ChangeProductStatusAsync(
                                id,
                                Status.Deleted,
                                true
                            );
                        }
                        else
                        {
                            await _productRepository.ChangeProductStatusAsync(
                                id,
                                Status.Deleted,
                                false
                            );
                        }

                        break;
                    }

                default:
                    throw new Exception("Invalid status");
            }
        }


        private async Task<bool> HasProductInOrdersAsync(Guid productId)
        {
            try
            {
                var response = await _orderClient
            .GetAsync($"api/OrderDetails/check-product/{productId}");

                var content = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"[ORDER API] Status: {response.StatusCode}");
                Console.WriteLine($"[ORDER API] Body: {content}");

                if (!response.IsSuccessStatusCode)
                    return true;

                return bool.Parse(content);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ORDER API ERROR] " + ex.Message);
                return true;
            }
        }


        private async Task<bool> HasProductInImportsAsync(Guid productId)
        {
            try
            {
                var response = await _importClient
            .GetAsync($"api/ImportStock/check-product/{productId}");

                var content = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"[IMPORT API] Status: {response.StatusCode}");
                Console.WriteLine($"[IMPORT API] Body: {content}");

                if (!response.IsSuccessStatusCode)
                    return true;

                return bool.Parse(content);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[IMPORT API ERROR] " + ex.Message);
                return true;
            }
        }

        public async Task<List<ReadProductDTOForCustomer>> GetAllProductAsync(ODataQueryOptions<ReadProductDTOForCustomer> queryOptions)
        {
            var query = _productRepository
                .GetAllProduct()
                .ProjectTo<ReadProductDTOForCustomer>(_mapper.ConfigurationProvider);

            var filtered = (IQueryable<ReadProductDTOForCustomer>)queryOptions.ApplyTo(query);

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


        public async Task<PagedResult<ReadProductDTO>> GetAllProductAdminAsync(
    ProductSpecification spec)
        {
            var (items, total) = await _productRepository.GetAllProductAdminAsync(spec);

            var productDTOs = _mapper.Map<IEnumerable<ReadProductDTO>>(items).ToList();

            if (productDTOs.Any())
            {
                var productIds = productDTOs.Select(p => p.ProductId).ToList();

                var stocks = await GetStocksFromInventory(productIds);

                var stockDict = stocks
                    .GroupBy(x => x.ProductId)
                    .ToDictionary(g => g.Key, g => g.First());

                foreach (var p in productDTOs)
                {
                    p.StockQuantity = stockDict.TryGetValue(p.ProductId, out var s)
                        ? s.QuantityAvailable
                        : 0;
                }
            }

            return new PagedResult<ReadProductDTO>(
                productDTOs,
                total,
                spec.Page,
                spec.PageSize);
        }

        private async Task<List<StockDto>> GetStocksFromInventory(List<Guid> productIds)
        {
            if (productIds == null || !productIds.Any())
                return new List<StockDto>();

            try
            {
                var response = await _inventoryClient.PostAsJsonAsync(
            "api/Inventory/stocks",
            productIds);

                var content = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"[INVENTORY STOCK] Status: {response.StatusCode}");
                Console.WriteLine($"[INVENTORY STOCK] Body: {content}");

                if (!response.IsSuccessStatusCode)
                    return new List<StockDto>();

                return await response.Content.ReadFromJsonAsync<List<StockDto>>()
                       ?? new List<StockDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine("[INVENTORY STOCK ERROR] " + ex.Message);
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
            bool productHasExist = await _productRepository.CheckProductExistAsync(updateproduct.ProductName, updateproduct.BrandId.Value, updateproduct.CategoryId.Value);

            if (productHasExist &&
               (product.ProductName != updateproduct.ProductName ||
                product.BrandId != updateproduct.BrandId ||
                product.CategoryId != updateproduct.CategoryId))
            {
                throw new InvalidOperationException("Product already exists");
            }

            _mapper.Map(updateproduct, product);

            product.UpdateAt = DateTime.UtcNow;

            product.ProductImages ??= new List<ProductImage>();
            product.ProductAttributes ??= new List<ProductAttribute>();

            // 1️⃣ xử lý ảnh bị xoá
            var existingImages = updateproduct.ExistingImages ?? new List<string>();

            var currentImages = product.ProductImages
     .Where(x => x.IsActive == true)
     .ToList();

            bool hasOrder = await HasProductInOrdersAsync(product.ProductId);
            bool hasImport = await HasProductInImportsAsync(product.ProductId);

            bool canReplaceOldImages = !hasOrder && !hasImport;

            foreach (var img in currentImages)
            {
                bool imageStillExists = existingImages.Any(x =>
                    string.Equals(x, img.ImageUrl, StringComparison.OrdinalIgnoreCase));

                if (!imageStillExists)
                {
                    // product chưa từng xuất hiện trong order/import
                    // => cho phép xoá thật
                    if (canReplaceOldImages)
                    {
                        if (!string.IsNullOrEmpty(img.PublicId))
                        {
                            await _cloudinaryService.DeleteImageAsync(img.PublicId);
                        }

                        product.ProductImages.Remove(img);
                    }
                    else
                    {
                        // đã từng nằm trong order/import
                        // => giữ ảnh để history không mất
                        img.IsActive = false;
                    }
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

                    product.ProductImages.Add(new ProductImage
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

        public async Task<IEnumerable<ReadProductDTOForCustomer>> GetNewProductsAsync()
        {
            var products = await _productRepository.GetNewProductAsync();
            return _mapper.Map<List<ReadProductDTOForCustomer>>(products);
        }

        public async Task<IEnumerable<ReadProductDTOForCustomer>> GetHotProductsAsync()
        {
            List<Guid>? productIds;

            try
            {
                productIds = await _orderClient.GetFromJsonAsync<List<Guid>>("api/OrderDetails/hot-products");
            }
            catch (Exception ex)
            {
                return new List<ReadProductDTOForCustomer>();
            }

            if (productIds == null || !productIds.Any())
                return new List<ReadProductDTOForCustomer>();

            var products = await _productRepository.GetProductsByIdsAsync(productIds);

            return _mapper.Map<List<ReadProductDTOForCustomer>>(products);
        }

        public async Task<List<SelectProductDto>> GetProductSelectListAsync()
        {
            // Logic: Chỉ lấy sản phẩm chưa bị xóa và đang hoạt động
            return await _productRepository.GetActiveProductsAsync<SelectProductDto>(p => p.Status == Status.Active);
        }

        public async Task<List<SelectProductDto>> GetProductSelectListToViewAsync()
        {
            return await _productRepository.GetActiveProductsAsync<SelectProductDto>();
        }

        //Code Hồ mới thêm
        public async Task<ProductInternalDto?> GetInternalAsync(Guid productId)
        {
            var product = await _productRepository.GetByIdInternalAsync(productId);
            if (product == null) return null;
            return new ProductInternalDto
            {
                ProductName = product.ProductName,
                ImageUrl = product.ProductImages.FirstOrDefault()?.ImageUrl
            };
        }
        public async Task<List<ProductSnapshotResponseDto>> GetProductSnapshotsAsync(List<Guid> productIds)
        {
            var products = await _productRepository.GetProductsForSnapshotAsync(productIds);

            return _mapper.Map<List<ProductSnapshotResponseDto>>(products);
        }
    }
}
