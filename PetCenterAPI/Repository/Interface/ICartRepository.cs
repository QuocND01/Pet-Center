using PetCenterAPI.Models;

namespace PetCenterAPI.Repository.Interface
{
    /// <summary>Data access for the shopping cart.</summary>
    public interface ICartRepository
    {
        Task<Cart?> GetCartWithDetailsAsync(Guid customerId);
        Task<Cart> GetOrCreateCartAsync(Guid customerId);

        Task<CartDetail?> GetDetailByIdAsync(Guid cartDetailId);
        Task<CartDetail?> GetDetailByProductAsync(Guid cartId, Guid productId);

        Task<Product?> GetProductAsync(Guid productId);
        /// <summary>Available stock for a product (0 if there is no inventory row).</summary>
        Task<int> GetAvailableStockAsync(Guid productId);

        Task AddDetailAsync(CartDetail detail);
        void RemoveDetail(CartDetail detail);
        void RemoveDetails(IEnumerable<CartDetail> details);

        Task SaveChangesAsync();
    }
}
