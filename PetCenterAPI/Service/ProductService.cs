using AutoMapper;
using AutoMapper.QueryableExtensions;
using CloudinaryDotNet.Actions;
using Humanizer;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using PetCenterAPI.Common;
using PetCenterAPI.DTOs;
using PetCenterAPI.Models;
using PetCenterAPI.Repository;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service.Interface;
using System.Net;
using static PetCenterAPI.DTOs.Requests.Product.ProductRequestDTO;
using static PetCenterAPI.DTOs.Responses.Product.ProductAttributeResponseDTO;
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
            if (string.IsNullOrWhiteSpace(createProduct.ProductName))
            {
                throw new Exception("Product name is required");
            }
            createProduct.ProductName = createProduct.ProductName.Trim();
            if (createProduct.Attributes != null)
            {
                foreach (var attribute in createProduct.Attributes)
                {
                    if (string.IsNullOrWhiteSpace(attribute.AttributeValue))
                    {
                        throw new InvalidOperationException("Attribute value is required.");
                    }

                    attribute.AttributeValue = attribute.AttributeValue.Trim();
                }
            }
            bool productHasExist = false;
            var compareAttributes = createProduct.Attributes
                .Select(x => new ProductAttributeCompareDTO
                {
                    CategoryAttributeId = x.CategoryAttributeId,
                    AttributeValue = x.AttributeValue
                })
                .ToList();
            productHasExist = await _productRepository.CheckProductExistAsync(createProduct.ProductName, createProduct.BrandId!.Value, createProduct.CategoryId!.Value, compareAttributes);
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
                var uploadedImages = new List<ImageUploadResult>();
                if (createProduct.ImageFiles.Count > 10)
                {
                    throw new BadHttpRequestException("Maximum 10 images are allowed.");
                }
                try
                {
                    if (createProduct.ImageFiles != null && createProduct.ImageFiles.Any())
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                        foreach (var file in createProduct.ImageFiles)
                        {
                            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                            if (!allowedExtensions.Contains(extension))
                            {
                                throw new InvalidOperationException(
                                    "Only JPG, JPEG, PNG, and WEBP images are allowed.");
                            }

                            if (!file.ContentType.StartsWith("image/"))
                            {
                                throw new InvalidOperationException("Invalid image file.");
                            }

                            // Giới hạn 5MB mỗi ảnh
                            if (file.Length > 5 * 1024 * 1024)
                            {
                                throw new InvalidOperationException(
                                    "Each image size cannot exceed 5 MB.");
                            }
                        }

                        var uploadTasks = createProduct.ImageFiles
                            .Select(file => _cloudinaryService.UploadImageAsync(file, "products"));

                        uploadedImages = (await Task.WhenAll(uploadTasks)).ToList();

                        foreach (var uploadResult in uploadedImages)
                        {
                            if (uploadResult == null ||
                                uploadResult.StatusCode != HttpStatusCode.OK)
                            {
                                throw new Exception("Failed to upload image");
                            }

                            product.ProductImages.Add(new ProductImage
                            {
                                ImageId = Guid.NewGuid(),
                                ImageUrl = uploadResult.SecureUrl.ToString(),
                                PublicId = uploadResult.PublicId,
                                IsActive = true
                            });
                        }
                    }

                    await _productRepository.AddProductAsync(product);
                }
                catch
                {
                    // Rollback Cloudinary
                    var deleteTasks = uploadedImages
                        .Where(x => x != null && !string.IsNullOrWhiteSpace(x.PublicId))
                        .Select(x => _cloudinaryService.DeleteImageAsync(x.PublicId));

                    await Task.WhenAll(deleteTasks);

                    throw;
                }
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
                        bool InOrder = await _productRepository.IsProductInOrderAsync(id);
                        if (!InOrder)
                        {
                            await _productRepository.ChangeProductStatusAsync(id, Status.Deleted);

                            // 2️⃣ mark images for cleanup (NO DELETE)
                            foreach (var image in product.ProductImages)
                            {
                                image.IsActive = false;
                                image.InactiveAt = DateTime.UtcNow;
                            }
                            await _productRepository.SaveAsync();
                        }
                        else
                        {
                            throw new BadHttpRequestException("Product is being used in an uncomplete order need complete order before deleted.");
                        }
                        break;
                    }

                default:
                    throw new Exception("Invalid status");
            }
        }


        public async Task<List<ReadProductDTOForCustomer>> GetAllProductAsync()
        {
            var products = await _productRepository.GetAllProduct();

            return _mapper.Map<List<ReadProductDTOForCustomer>>(products);
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

            if (string.IsNullOrWhiteSpace(updateproduct.ProductName))
            {
                throw new Exception("Product name is required");
            }

            updateproduct.ProductName = updateproduct.ProductName.Trim();

            if (updateproduct.Attributes != null)
            {
                foreach (var attribute in updateproduct.Attributes)
                {
                    if (string.IsNullOrWhiteSpace(attribute.AttributeValue))
                    {
                        throw new InvalidOperationException("Attribute value is required.");
                    }

                    attribute.AttributeValue = attribute.AttributeValue.Trim();
                }
            }

            if (updateproduct.BrandId == null || updateproduct.CategoryId == null)
                throw new Exception("BrandId and CategoryId are required");
            var compareAttributes = updateproduct.Attributes
               .Select(x => new ProductAttributeCompareDTO
               {
                   CategoryAttributeId = x.CategoryAttributeId,
                   AttributeValue = x.AttributeValue
               })
               .ToList();
            bool productHasExist = await _productRepository.CheckProductExistAsync(updateproduct.ProductName, updateproduct.BrandId.Value, updateproduct.CategoryId.Value, compareAttributes, id);
            Console.WriteLine(productHasExist);
            if (productHasExist)
            {
                throw new InvalidOperationException("Product already exists");
            }

            _mapper.Map(updateproduct, product);

            product.UpdateAt = DateTime.UtcNow;

            product.ProductImages ??= new List<ProductImage>();
            product.ProductAttributes ??= new List<ProductAttribute>();

            var uploadedImages = new List<ImageUploadResult>();
            var finalImageCount =
             (updateproduct.ExistingImages?.Count ?? 0) +
                (updateproduct.ImageFiles?.Count ?? 0);

            if (finalImageCount > 10)
            {
                throw new BadHttpRequestException("Maximum 10 images are allowed.");
            }
            try
            {
                // 1️⃣ Xử lý ảnh bị xóa
                var existingImages = updateproduct.ExistingImages ?? new List<string>();

                var currentImages = product.ProductImages
                    .Where(x => x.IsActive)
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

                // 2️⃣ Upload ảnh mới song song
                if (updateproduct.ImageFiles != null && updateproduct.ImageFiles.Any())
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                    foreach (var file in updateproduct.ImageFiles)
                    {
                        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                        if (!allowedExtensions.Contains(extension))
                        {
                            throw new InvalidOperationException(
                                "Only JPG, JPEG, PNG, and WEBP images are allowed.");
                        }

                        if (!file.ContentType.StartsWith("image/"))
                        {
                            throw new InvalidOperationException("Invalid image file.");
                        }

                        // Giới hạn 5MB mỗi ảnh
                        if (file.Length > 5 * 1024 * 1024)
                        {
                            throw new InvalidOperationException(
                                "Each image size cannot exceed 5 MB.");
                        }
                    }

                    var uploadTasks = updateproduct.ImageFiles
                        .Select(file => _cloudinaryService.UploadImageAsync(file, "products"));

                    uploadedImages = (await Task.WhenAll(uploadTasks)).ToList();

                    foreach (var uploadResult in uploadedImages)
                    {
                        if (uploadResult == null ||
                            uploadResult.StatusCode != HttpStatusCode.OK)
                        {
                            throw new Exception("Failed to upload image");
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

                // 3️⃣ Update attributes
                if (updateproduct.Attributes != null)
                {
                    var existingAttrs = product.ProductAttributes ??= new List<ProductAttribute>();

                    foreach (var newAttr in updateproduct.Attributes)
                    {
                        var match = existingAttrs.FirstOrDefault(a =>
                            a.CategoryAttributeId == newAttr.CategoryAttributeId);

                        if (match == null)
                            throw new InvalidOperationException("Attribute does not exist.");

                        match.AttributeValue = newAttr.AttributeValue;
                        match.IsActive = true;
                    }
                }

                await _productRepository.UpdateProductAsync(product);
            }
            catch
            {
                // Rollback các ảnh mới upload
                var deleteTasks = uploadedImages
                    .Where(x => x != null && !string.IsNullOrWhiteSpace(x.PublicId))
                    .Select(x => _cloudinaryService.DeleteImageAsync(x.PublicId));

                await Task.WhenAll(deleteTasks);

                throw;
            }
        }

        public async Task<IEnumerable<ReadProductDTOForCustomer>> GetNewProductsAsync()
        {
            var products = await _productRepository.GetNewProductAsync();
            return _mapper.Map<List<ReadProductDTOForCustomer>>(products);
        }

        public async Task<IEnumerable<ReadProductDTOForCustomer>> GetHotProductsAsync()
        {

            var products = await _productRepository.GetHotProduct();
            return _mapper.Map<List<ReadProductDTOForCustomer>>(products);
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
    }
}
