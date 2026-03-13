using PetCenterClient.DTOs;

namespace PetCenterClient.Services.Interface
{
    public interface ICartService
    {
        Task<CartResponseDTO?> GetCartAsync(Guid customerId);
        Task<(bool Success, string Message)> AddToCartAsync(AddToCartRequestDTO dto);
        Task<(bool Success, string Message)> UpdateCartDetailAsync(Guid cartDetailId, int quantity);
        Task<(bool Success, string Message)> DeleteCartDetailAsync(Guid cartDetailId);
        Task<(bool Success, string Message)> ClearCartAsync(Guid customerId);
    }
}