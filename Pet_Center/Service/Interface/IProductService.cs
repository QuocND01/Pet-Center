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
    }
}
