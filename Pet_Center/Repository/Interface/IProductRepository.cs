using ProductAPI.DTOs;
using ProductAPI.Models;
using System.Linq.Expressions;

namespace ProductAPI.Repository.Interface
{
    public interface IProductRepository
    {
        IQueryable<Product> GetAllProduct();
       
        Task<Product?> GetProductByIdAsync(Guid id);
        Task AddProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(Guid id);

        Task DeleteProductAttributesByProductIdAsync(Guid productId);

        Task<bool> CheckProductExist(string productName, Guid brandId, Guid categoryId);

        Task<List<T>> GetActiveProductsAsync<T>(Expression<Func<Product, bool>>? filter = null);

        Task<IEnumerable<Product?>> GetNewProduct();

        Task<IEnumerable<Product?>> GetProductsByIds(List<Guid> ids);


        //get by list
        Task<List<Product>> GetByIds(List<Guid> ids);
        Task SaveChangesAsync();

        Task<bool> DecreaseStockAsync(Guid productId, int quantity);
        Task<bool> IncreaseStockAsync(Guid productId, int quantity);

        Task<Product?> GetProductByIdIncludeDeletedAsync(Guid id);
    }
}
