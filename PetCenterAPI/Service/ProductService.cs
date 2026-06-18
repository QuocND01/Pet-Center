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
using static PetCenterAPI.DTOs.Requests.Product.ProductRequestDTO;
using static PetCenterAPI.DTOs.Responses.Product.ProductResponseDTO;

namespace PetCenterAPI.Service
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly HttpClient _orderClient;
        private readonly IMapper _mapper;
        public ProductService(IProductRepository productRepository, IMapper mapper, ICloudinaryService service, IHttpClientFactory httpClientFactory)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _cloudinaryService = service;
            _orderClient = httpClientFactory.CreateClient("OrdersAPI");
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
                        // 1️⃣ update product status only
                        await _productRepository.ChangeProductStatusAsync(id, Status.Deleted);

                        // 2️⃣ mark images for cleanup (NO DELETE)
                        foreach (var image in product.ProductImages)
                        {
                            image.IsActive = false;
                            image.InactiveAt = DateTime.UtcNow;
                        }
                        await _productRepository.SaveAsync();

                        break;
                    }

                default:
                    throw new Exception("Invalid status");
            }
        }


        public async Task<List<ReadProductDTOForCustomer>> GetAllProductAsync(
     ODataQueryOptions<ReadProductDTOForCustomer> queryOptions)
        {
            var query = _productRepository
                .GetAllProduct()
                .ProjectTo<ReadProductDTOForCustomer>(_mapper.ConfigurationProvider);

            var filtered = (IQueryable<ReadProductDTOForCustomer>)
                queryOptions.ApplyTo(query);

            return await filtered.ToListAsync();
        }


        public async Task<PagedResult<ReadProductDTO>> GetAllProductAdminAsync(
    ProductSpecification spec)
        {
            var (items, total) = await _productRepository.GetAllProductAdminAsync(spec);

            var productDTOs = _mapper.Map<IEnumerable<ReadProductDTO>>(items).ToList();

            return new PagedResult<ReadProductDTO>(
                productDTOs,
                total,
                spec.Page,
                spec.PageSize);
        }



        public async Task<ReadProductDTO> GetProductByIdAsync(Guid id)
        {
            var product = await _productRepository.GetProductByIdAsync(id);

            if (product == null)
                throw new KeyNotFoundException("Product not found");

            var result = _mapper.Map<ReadProductDTO>(product);

            return result;
        }

        public async Task UpdateProductAsync(Guid id, UpdateProductDTO updateproduct)
        {
            var product = await _productRepository.GetProductByIdAsync(id);

            if (product == null)
                throw new KeyNotFoundException("Product not found");

            if (updateproduct.BrandId == null || updateproduct.CategoryId == null)
                throw new Exception("BrandId and CategoryId are required");
            Console.WriteLine(updateproduct.ProductName);
            Console.WriteLine(updateproduct.BrandId);
            Console.WriteLine(updateproduct.CategoryId);

            bool productHasExist = await _productRepository.CheckProductExistAsync(updateproduct.ProductName, updateproduct.BrandId.Value, updateproduct.CategoryId.Value, id);
            Console.WriteLine(productHasExist);
            if (productHasExist)
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

            foreach (var img in currentImages)
            {
                bool stillExists = existingImages.Any(x =>
                    string.Equals(x, img.ImageUrl, StringComparison.OrdinalIgnoreCase));

                if (!stillExists)
                {
                    img.IsActive = false;
                    img.InactiveAt = DateTime.UtcNow;
                }
            }

            // 2️⃣ upload ảnh mới
            if (updateproduct.ImageFiles != null && updateproduct.ImageFiles.Any())
            {
                foreach (var file in updateproduct.ImageFiles)
                {
                    var uploadResult = await _cloudinaryService.UploadImageAsync(file, "products");

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

                var newAttrSet = updateproduct.Attributes
     .Select(a => a.CategoryAttributeId)
     .ToHashSet();

                foreach (var oldAttr in existingAttrs.Where(x => x.IsActive == true))
                {
                    if (!newAttrSet.Contains(oldAttr.CategoryAttributeId))
                    {
                        oldAttr.IsActive = false;
                    }
                }

                foreach (var newAttr in updateproduct.Attributes)
                {
                    var match = existingAttrs.FirstOrDefault(a =>
                        a.CategoryAttributeId == newAttr.CategoryAttributeId);

                    if (match == null)
                    {
                        existingAttrs.Add(new ProductAttribute
                        {
                            ProductId = product.ProductId,
                            CategoryAttributeId = newAttr.CategoryAttributeId,
                            AttributeValue = newAttr.AttributeValue,
                            IsActive = true
                        });
                    }
                    else
                    {
                        match.AttributeValue = newAttr.AttributeValue;
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

        //public async Task<List<SelectProductDto>> GetProductSelectListAsync()
        //{
        //    // Logic: Chỉ lấy sản phẩm chưa bị xóa và đang hoạt động
        //    return await _productRepository.GetActiveProductsAsync<SelectProductDto>(p => p.Status == Status.Active);
        //}

        //public async Task<List<SelectProductDto>> GetProductSelectListToViewAsync()
        //{
        //    return await _productRepository.GetActiveProductsAsync<SelectProductDto>();
        //}

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
        //public async Task<List<ProductSnapshotResponseDto>> GetProductSnapshotsAsync(List<Guid> productIds)
        //{
        //    var products = await _productRepository.GetProductsForSnapshotAsync(productIds);

        //    return _mapper.Map<List<ProductSnapshotResponseDto>>(products);
        //}
    }
}
