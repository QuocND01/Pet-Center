using PetCenterClient.DTOs;

namespace PetCenterClient.Services.Interface
{
    public interface IProductAPIClient
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

        Task<PagedResponse<ReadProductDTO>> GetAllProductAdminAsync(
       string? search,
       bool? isActive,
       decimal? minPrice,
       decimal? maxPrice,
       Guid? categoryId,
       Guid? brandId,
       string? sortBy,
       DateTime? fromDate,
       DateTime? toDate,
       string sortOrder = "asc",
       int page = 1,
       int pageSize = 10);

        Task<ReadProductDTO> DetailsProductAsync(Guid? id);
        Task AddProductAsync(CreateProductDTO createproduct);
        Task UpdateProductAsync(Guid? id, UpdateProductDTO updateproduct);
        Task DeleteProductAsync(Guid? id);

        Task<List<ReadProductDTO>> GetHotProductsAsync();
        Task<List<ReadProductDTO>> GetNewProductsAsync();
        Task<List<ProductSelectDto>> GetProductSelectAsync();
        Task<List<ProductSelectDto>> GetProductSelectToViewAsync();
        Task IncreaseStockBulkAsync(List<IncreaseStockItemDto> items);
        Task<bool> DecreaseStockAsync(Guid productId, int quantity);
        Task<bool> IncreaseStockAsync(Guid productId, int quantity);
        Task<ReadProductDTO> GetProductByIdIncludeDeletedAsync(Guid? id);
    }
}
