using Microsoft.AspNetCore.OData.Query;
using PetCenterAPI.Common;
using PetCenterAPI.DTOs;
using PetCenterAPI.Models;
using static PetCenterAPI.DTOs.Requests.Product.ProductRequestDTO;
using static PetCenterAPI.DTOs.Responses.Product.ProductResponseDTO;

namespace PetCenterAPI.Service.Interface
{
    public interface IProductService
    {
        Task<List<ReadProductDTOForCustomer>> GetAllProductAsync();

        Task<PagedResult<ReadProductDTO>> GetAllProductAdminAsync(
    ProductSpecification spec);
        Task<ReadProductDTO> GetProductByIdAsync(Guid id);
        Task AddProductAsync(CreateProductDTO createproduct);
        Task UpdateProductAsync(Guid id, UpdateProductDTO updateproduct);
        Task ChangeProductStatusAsync(Guid id, Status status);

        Task<IEnumerable<ReadProductDTOForCustomer>> GetNewProductsAsync();
        Task<IEnumerable<ReadProductDTOForCustomer>> GetHotProductsAsync();

       
        // Code mới Hồ mới thêm
        Task<ProductInternalDto?> GetInternalAsync(Guid productId);

    }
}
