using OrdersAPI.Models;

namespace OrdersAPI.Repository.Interface
{
    public interface ICartRepository
    {
        Task<Cart?> GetCartByCustomerIdAsync(Guid customerId);
        Task<Cart> CreateCartAsync(Guid customerId);
        Task<CartDetail?> GetCartDetailByIdAsync(Guid cartDetailId);
        Task<CartDetail?> GetCartDetailByProductAsync(Guid cartId, Guid productId);
        Task AddCartDetailAsync(CartDetail cartDetail);
        Task UpdateCartDetailAsync(CartDetail cartDetail);
        Task DeleteCartDetailAsync(CartDetail cartDetail);
        Task ClearCartAsync(Guid cartId);
        Task<bool> SaveChangesAsync();
    }
}