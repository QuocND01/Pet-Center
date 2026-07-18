using PetCenterAPI.Common;
using PetCenterAPI.DTOs;
using PetCenterAPI.Models;
using System.Linq.Expressions;
using static PetCenterAPI.DTOs.Requests.Product.ProductAttributeRequestDTO;
using static PetCenterAPI.DTOs.Responses.Product.ProductAttributeResponseDTO;

namespace PetCenterAPI.Repository.Interface
{
    public interface IProductRepository
    {
        Task<List<Product>> GetAllProduct();

        Task<(IEnumerable<Product> Items, int Total)> GetAllProductAdminAsync(
    ProductSpecification spec);

        Task<Product?> GetProductByIdAsync(Guid id);
        Task AddProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task ChangeProductStatusAsync(
     Guid id,
     Status status,
     bool hardDeleteImages = false);
        Task<bool> CheckProductExistAsync(string productName,Guid brandId,Guid categoryId, List<ProductAttributeCompareDTO> attributes, Guid? excludeId = null);

        Task<List<T>> GetActiveProductsAsync<T>(Expression<Func<Product, bool>>? filter = null);

        Task<IEnumerable<Product?>> GetNewProductAsync();


        Task<IEnumerable<Product?>> GetHotProduct();

        Task<Product?> GetByIdInternalAsync(Guid productId);

        Task<List<Product>> GetProductsForSnapshotAsync(List<Guid> productIds);

        Task<bool> IsProductInOrderAsync(Guid productId);

        Task SaveAsync();
    }
}
