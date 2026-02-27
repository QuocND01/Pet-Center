using AutoMapper;
using Pet_Center.DTOs;
using Pet_Center.Models;
using Pet_Center.Repository.Interface;
using Pet_Center.Service.Interface;

namespace Pet_Center.Service
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        public ProductService(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }
        public Task AddProductAsync(CreateProductDTO createproduct)
        {
            var product = _mapper.Map<Product>(createproduct);

            return _productRepository.AddProductAsync(product);
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
