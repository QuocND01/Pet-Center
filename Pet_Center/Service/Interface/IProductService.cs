using ProductAPI.DTOs;

namespace ProductAPI.Service.Interface
{
    public interface IProductService
    {
        Task<IEnumerable<ReadProductDTO>> GetAllProductAsync();
        Task<ReadProductDTO> GetProductByIdAsync(Guid id);
        Task AddProductAsync(CreateProductDTO createproduct);
        Task UpdateProductAsync(Guid id, UpdateProductDTO updateproduct);
        Task DeleteProductAsync(Guid id);
    }
}
