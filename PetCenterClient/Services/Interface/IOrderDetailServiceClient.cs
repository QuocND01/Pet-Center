using PetCenterClient.DTOs;

namespace PetCenterClient.Services.Interface
{
    public interface IOrderDetailServiceClient
    {
        Task<List<OrderDetailResponseDTO>> GetByOrderIdAsync(Guid orderId);
        Task<bool> UpdateAsync(Guid id, OrderDetailRequestDTO dto);
    }
}