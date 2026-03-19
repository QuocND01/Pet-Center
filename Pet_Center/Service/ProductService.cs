using AutoMapper;
using AutoMapper.QueryableExtensions;
using Humanizer;
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
        private readonly HttpClient _httpClient;
        private readonly IMapper _mapper;
        public ProductService(IProductRepository productRepository, IMapper mapper, ICloudinaryService service, HttpClient httpClient)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _cloudinaryService = service;
            _httpClient = httpClient;
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

        public  IQueryable<ReadProductDTO> GetAllProduct()
        {
            return _productRepository.GetAllProduct().ProjectTo<ReadProductDTO>(_mapper.ConfigurationProvider);
        }


        public async Task<ReadProductDTO> GetProductByIdAsync(Guid id)
        {
            var product = await _productRepository.GetProductByIdAsync(id);
            return _mapper.Map<ReadProductDTO>(product);
        }

        public async Task UpdateProductAsync(Guid id, UpdateProductDTO updateproduct)
        {
            var product = await _productRepository.GetProductByIdAsync(id);

            if (product == null)
                throw new Exception("Product not found");

            bool productHasExist = await _productRepository.CheckProductExist(updateproduct.ProductName,updateproduct.BrandId.Value,updateproduct.CategoryId.Value);

            if (productHasExist &&
               (product.ProductName != updateproduct.ProductName ||
                product.BrandId != updateproduct.BrandId ||
                product.CategoryId != updateproduct.CategoryId))
            {
                throw new InvalidOperationException("Product already exists");
            }

            _mapper.Map(updateproduct, product);

            product.UpdateAt = DateTime.Now;

            product.Images ??= new List<Image>();
            product.ProductAttributes ??= new List<ProductAttribute>();

            // 1️⃣ xử lý ảnh bị xoá
            var existingImages = updateproduct.ExistingImages ?? new List<string>();

            var currentImages = product.Images.ToList();

            foreach (var img in currentImages)
            {
                if (!existingImages.Contains(img.ImageUrl))
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
                await _productRepository.DeleteProductAttributesByProductIdAsync(product.ProductId);

                foreach (var attr in updateproduct.Attributes)
                {
                    var entity = _mapper.Map<ProductAttribute>(attr);
                    entity.ProductId = product.ProductId;

                    product.ProductAttributes.Add(entity);
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
            var productIds = await _httpClient.GetFromJsonAsync<List<Guid>>(
                "https://localhost:7007/api/OrderDetails/hot-products");

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


        public async Task IncreaseStockBulk(List<IncreaseStockItemDto> items)
        {
            if (items == null || !items.Any()) return;

            // 🔥 gộp product trùng
            var grouped = items
                .GroupBy(x => x.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .ToList();

            var ids = grouped.Select(x => x.ProductId).ToList();

            var products = await _productRepository.GetByIds(ids);

            foreach (var item in grouped)
            {
                var product = products.FirstOrDefault(x => x.ProductId == item.ProductId);

                if (product == null) continue;

                product.StockQuantity ??= 0;
                product.StockQuantity += item.Quantity;
                product.UpdateAt = DateTime.UtcNow;
            }

            await _productRepository.SaveChangesAsync();
        }
        public async Task DecreaseStockBulk(List<DecreaseStockItemDto> items)
        {
            if (items == null || !items.Any()) return;

            // ❗ validate
            if (items.Any(x => x.Quantity <= 0))
                throw new Exception("Quantity must be greater than 0");

            // 🔥 gộp product trùng
            var grouped = items
                .GroupBy(x => x.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .ToList();

            var ids = grouped.Select(x => x.ProductId).ToList();

            var products = await _productRepository.GetByIds(ids);

            // 🔥 tối ưu lookup
            var productDict = products.ToDictionary(x => x.ProductId);

            foreach (var item in grouped)
            {
                if (!productDict.TryGetValue(item.ProductId, out var product))
                    throw new Exception($"Product {item.ProductId} not found");

                product.StockQuantity ??= 0;

                // ❗ check đủ hàng
                if (product.StockQuantity < item.Quantity)
                    throw new Exception($"Product {item.ProductId} is out of stock");

                product.StockQuantity -= item.Quantity;
                product.UpdateAt = DateTime.UtcNow;
            }

            await _productRepository.SaveChangesAsync();
        }

        public async Task<bool> DecreaseStockAsync(Guid productId, int quantity)
        {
            return await _productRepository.DecreaseStockAsync(productId, quantity);
        }

        public async Task<bool> IncreaseStockAsync(Guid productId, int quantity)
        {
            // Chỗ này sau này bạn có thể cắm thêm logic ghi log: "Sản phẩm A được cộng lại X cái do đơn hàng Y bị hủy"
            return await _productRepository.IncreaseStockAsync(productId, quantity);
        }
        public async Task<ReadProductDTO> GetProductByIdIncludeDeletedAsync(Guid id)
        {
            var product = await _productRepository.GetProductByIdIncludeDeletedAsync(id);
            return _mapper.Map<ReadProductDTO>(product);
        }
    }
}
