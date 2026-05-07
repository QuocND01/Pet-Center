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

        Task<IEnumerable<ReadProductDTO>> GetNewProductsAsync();
        Task<IEnumerable<ReadProductDTO>> GetHotProductsAsync();
    }
}
