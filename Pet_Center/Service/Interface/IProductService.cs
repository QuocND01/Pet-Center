using ProductAPI.DTOs;
using ProductAPI.Models;

namespace ProductAPI.Service.Interface
{
    public interface IProductService
    {
        IQueryable<ReadProductDTO> GetAllProduct();

        Task<ReadProductDTO> GetProductByIdAsync(Guid id);
        Task AddProductAsync(CreateProductDTO createproduct);
        Task UpdateProductAsync(Guid id, UpdateProductDTO updateproduct);
        Task DeleteProductAsync(Guid id);
        Task<List<SelectProductDto>> GetProductSelectListAsync();

        Task<IEnumerable<ReadProductDTO>> GetNewProducts();
        Task<IEnumerable<ReadProductDTO>> GetHotProducts();

        Task IncreaseStockBulk(List<IncreaseStockItemDto> items);
        Task<bool> DecreaseStockAsync(Guid productId, int quantity);

        Task<bool> IncreaseStockAsync(Guid productId, int quantity);
    }
}
