using PetCenterClient.DTOs;

namespace PetCenterClient.Services.Interface
{
    public interface IProductService
    {
        Task<OdataResponse<ReadProductDTO>> GetAllProductAsync(
                string? search,
                bool? isActive,
                decimal? minPrice,
                decimal? maxPrice,
                DateTime? fromDate,
                DateTime? toDate,
                string? sortBy,
                Guid? categoryid,
                Guid? brandid,
                string sortOrder = "asc",
                int page = 1);

        Task<ReadProductDTO> GetProductByIdAsync(Guid? id);
        Task AddProductAsync(CreateProductDTO createproduct);
        Task UpdateProductAsync(Guid? id, UpdateProductDTO updateproduct);
        Task DeleteProductAsync(Guid? id);

        Task<List<ReadProductDTO>> GetHotProductsAsync();
        Task<List<ReadProductDTO>> GetNewProductsAsync();
        Task<List<ProductSelectDto>> GetProductSelectAsync();
    }
}
