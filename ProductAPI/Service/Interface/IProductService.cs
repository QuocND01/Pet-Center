using Microsoft.AspNetCore.OData.Query;
using ProductAPI.DTOs;
using ProductAPI.Models;

namespace ProductAPI.Service.Interface
{
    public interface IProductService
    {
        Task<List<ReadProductDTO>> GetAllProductAsync(ODataQueryOptions<ReadProductDTO> queryOptions);

        Task<ReadProductDTO> GetProductByIdAsync(Guid id);
        Task AddProductAsync(CreateProductDTO createproduct);
        Task UpdateProductAsync(Guid id, UpdateProductDTO updateproduct);
        Task DeleteProductAsync(Guid id);
        Task<List<SelectProductDto>> GetProductSelectListAsync();

        Task<List<SelectProductDto>> GetProductSelectListToViewAsync();

        Task<IEnumerable<ReadProductDTO>> GetNewProducts();
        Task<IEnumerable<ReadProductDTO>> GetHotProducts();

        //Task IncreaseStockBulk(List<IncreaseStockItemDto> items);
        //Task<bool> DecreaseStockAsync(Guid productId, int quantity);

        //Task<bool> IncreaseStockAsync(Guid productId, int quantity);
        //Task<bool> IncreaseStockAsync(Guid productId, int quantity);

        //Task<ReadProductDTO> GetProductByIdIncludeDeletedAsync(Guid id);

        // Code mới Hồ mới thêm
        Task<ProductInternalDto?> GetInternalAsync(Guid productId);
    }
}
