using ProductAPI.Models;

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
    }
}
