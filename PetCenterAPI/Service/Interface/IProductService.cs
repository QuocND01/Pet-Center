using Microsoft.AspNetCore.OData.Query;
using PetCenterAPI.Common;
using PetCenterAPI.DTOs;
using PetCenterAPI.Models;

namespace PetCenterAPI.Service.Interface
{
    public interface IProductService
    {
        Task<List<ReadProductDTOForCustomer>> GetAllProductAsync(ODataQueryOptions<ReadProductDTOForCustomer> queryOptions);

        Task<PagedResult<ReadProductDTO>> GetAllProductAdminAsync(
    ProductSpecification spec);
        Task<ReadProductDTO> GetProductByIdAsync(Guid id);
        Task AddProductAsync(CreateProductDTO createproduct);
        Task UpdateProductAsync(Guid id, UpdateProductDTO updateproduct);
        Task ChangeProductStatusAsync(Guid id, Status status);
        Task<List<SelectProductDto>> GetProductSelectListAsync();

        Task<List<SelectProductDto>> GetProductSelectListToViewAsync();

        Task<IEnumerable<ReadProductDTOForCustomer>> GetNewProductsAsync();
        Task<IEnumerable<ReadProductDTOForCustomer>> GetHotProductsAsync();

       
        // Code mới Hồ mới thêm
        Task<ProductInternalDto?> GetInternalAsync(Guid productId);
        Task<List<ProductSnapshotResponseDto>> GetProductSnapshotsAsync(List<Guid> productIds);
    }
}
