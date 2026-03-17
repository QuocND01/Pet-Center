using OrdersAPI.DTOs;

namespace OrdersAPI.Service.Interface
{
    public interface ICartService
    {
        Task<CartResponseDTO?> GetCartByCustomerIdAsync(Guid customerId);
        Task<(bool Success, string Message)> AddToCartAsync(AddToCartRequestDTO dto);
        Task<(bool Success, string Message)> UpdateCartDetailAsync(Guid cartDetailId, int quantity);
        Task<(bool Success, string Message)> DeleteCartDetailAsync(Guid cartDetailId);
        Task<(bool Success, string Message)> ClearCartAsync(Guid customerId);
    }
}