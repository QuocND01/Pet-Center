using PetCenterAPI.DTOs.Requests.Cart;
using PetCenterAPI.DTOs.Responses.Cart;

namespace PetCenterAPI.Service.Interface
{
    /// <summary>Business logic for the customer shopping cart.</summary>
    public interface ICartService
    {
        Task<CartResponseDTO> GetCartAsync(Guid customerId);
        Task<(bool Success, string Message)> AddToCartAsync(Guid customerId, AddToCartRequestDTO request);
        Task<(bool Success, string Message)> UpdateDetailAsync(Guid customerId, Guid cartDetailId, UpdateCartDetailRequestDTO request);
        Task<(bool Success, string Message)> DeleteDetailAsync(Guid customerId, Guid cartDetailId);
        Task<(bool Success, string Message)> ClearCartAsync(Guid customerId);
    }
}
