using PetCenterClient.DTOs;

namespace PetCenterClient.Services.Interface
{
    public interface IOrderServiceClient
    {
        Task<List<OrderResponseDTO>> GetAllAsync();
        Task<OrderResponseDTO?> GetByIdAsync(Guid id);
        Task<bool> CreateAsync(OrderRequestDTO dto);
        Task<bool> UpdateAsync(Guid id, OrderRequestDTO dto);
        Task<bool> DeleteAsync(Guid id);
        Task<List<OrderDetailVM>> GetOrderDetailsAsync(Guid orderId);
    }
}
