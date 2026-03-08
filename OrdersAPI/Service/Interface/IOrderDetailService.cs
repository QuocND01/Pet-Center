using OrdersAPI.DTOs;

namespace OrdersAPI.Service.Interface
{
    public interface IOrderDetailService
    {
        Task<IEnumerable<OrderDetailResponseDTO>> GetDetailsByOrderAsync(Guid orderId);
        Task<bool> CreateDetailAsync(OrderDetailRequestDTO dto);
        Task<bool> UpdateDetailAsync(Guid id, OrderDetailRequestDTO dto);
        Task<bool> DeleteDetailAsync(Guid id);
    }
}
