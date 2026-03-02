using AutoMapper;
using ProductAPI.DTOs;
using ProductAPI.Models;
using ProductAPI.Repository.Interface;
using ProductAPI.Service.Interface;
using ProductAPI.Service.Interface;

namespace ProductAPI.Service
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IMapper _mapper;
        public ProductService(IProductRepository productRepository, IMapper mapper, ICloudinaryService service)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _cloudinaryService = service;
        }


        public async Task AddProductAsync(CreateProductDTO createProduct)
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



        public async Task DeleteProductAsync(Guid id)
        {
            await _productRepository.DeleteProductAsync(id);
        }

        public async Task<IEnumerable<ReadProductDTO>> GetAllProductAsync()
        {
            var products = await _productRepository.GetAllProductAsync();

            return _mapper.Map<IEnumerable<ReadProductDTO>>(products);
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

            _mapper.Map(updateproduct, product);

            await _productRepository.UpdateProductAsync(product);
        }   
    }
}
