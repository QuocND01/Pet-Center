using ProductAPI.Common;
using ProductAPI.DTOs;
using ProductAPI.Models;
using System.Linq.Expressions;

namespace ProductAPI.Repository.Interface
{
    public interface IProductRepository
    {
        IQueryable<Product> GetAllProduct();

        Task<(IEnumerable<Product> Items, int Total)> GetAllProductAdminAsync(
    ProductSpecification spec);

        Task<Product?> GetProductByIdAsync(Guid id);
        Task AddProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(Guid id);

        Task<bool> CheckProductExistAsync(string productName, Guid brandId, Guid categoryId);

        Task<List<T>> GetActiveProductsAsync<T>(Expression<Func<Product, bool>>? filter = null);

        Task<IEnumerable<Product?>> GetNewProductAsync();


         Task<IEnumerable<Product?>> GetProductsByIdsAsync(List<Guid> ids);
        //get by list
        //Task<List<Product>> GetByIds(List<Guid> ids);
        //Task SaveChangesAsync();

        //Task<bool> DecreaseStockAsync(Guid productId, int quantity);
        //Task<bool> IncreaseStockAsync(Guid productId, int quantity);

        //Task<Product?> GetProductByIdIncludeDeletedAsync(Guid id);

        Task<Product?> GetByIdInternalAsync(Guid productId);

        Task<List<Product>> GetProductsForSnapshotAsync(List<Guid> productIds);
    }
}
