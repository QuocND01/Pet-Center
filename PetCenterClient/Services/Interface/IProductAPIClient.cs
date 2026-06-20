using PetCenterClient.Common;
using PetCenterClient.DTOs;
using PetCenterClient.ViewModels.Common;
using PetCenterClient.ViewModels.Product;

namespace PetCenterClient.Services.Interface
{
    public interface IProductAPIClient
    {
        Task<OdataResponse<ReadProductViewModelForCustomer>> GetAllProductAsync(
                string? search,
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
       Status? status,
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

        Task<ReadProductViewModel> GetProductByIdIncludeDeletedAsync(Guid? id);
    }
}
