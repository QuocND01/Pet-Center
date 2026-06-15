using PetCenterClient.Common;
using PetCenterClient.DTOs;

namespace PetCenterClient.Services.Interface
{
    public interface IProductServiceClient
    {
        Task<OdataResponse<ReadProductViewModelForCustomer>> GetAllProductAsync(
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

        Task<PagedResponse<ReadProductViewModel>> GetAllProductAdminAsync(
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

        Task<ReadProductViewModel> DetailsProductAsync(Guid? id);
        Task AddProductAsync(CreateProductViewModel createproduct);
        Task UpdateProductAsync(Guid? id, UpdateProductViewModel updateproduct);
        Task ChangeProductStatusAsync(
      Guid id,
      Status status);

        Task<List<ReadProductViewModelForCustomer>> GetHotProductsAsync();
        Task<List<ReadProductViewModelForCustomer>> GetNewProductsAsync();
        Task<List<ProductSelectViewModel>> GetProductSelectAsync();
        Task<List<ProductSelectViewModel>> GetProductSelectToViewAsync();
        Task IncreaseStockBulkAsync(List<IncreaseStockItemDto> items);
        Task<bool> DecreaseStockAsync(Guid productId, int quantity);
        Task<bool> IncreaseStockAsync(Guid productId, int quantity);
        Task<ReadProductViewModel> GetProductByIdIncludeDeletedAsync(Guid? id);
    }
}
