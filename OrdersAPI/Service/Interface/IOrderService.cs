using OrdersAPI.DTOs;

namespace OrdersAPI.Service.Interface
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderResponseDTO>> GetAllOrdersAsync();
        Task<OrderResponseDTO?> GetOrderByIdAsync(Guid id);
        Task<OrderResponseDTO> CreateOrderAsync(OrderRequestDTO dto);
        Task<bool> UpdateOrderAsync(Guid id, OrderRequestDTO dto);
        Task<bool> DeleteOrderAsync(Guid id);
    }
}
