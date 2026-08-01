using Microsoft.AspNetCore.OData.Query;
using PetCenterAPI.Common;
using PetCenterAPI.DTOs.Requests.Order;
using static PetCenterAPI.DTOs.Requests.Order.OrderRequestDTO;
namespace PetCenterAPI.Service.Interface
{
    public interface IOrderService
    {
        Task<List<ReadOrderListDTO>> GetOrderListAdminAsync(ODataQueryOptions<ReadOrderListDTO> queryOptions);

        // Tìm kiếm theo một chuỗi con của OrderId (partial match)
        Task<List<ReadOrderListDTO>> SearchOrdersByPartialIdAsync(string term);

        Task<ReadOrderDetailDTO?> GetOrderDetailsAsync(Guid orderId);

        Task<bool> CancelOrderAsync(Guid orderId);
        Task<int> AdvanceOrderStatusAsync(Guid orderId, Guid? staffId = null);
        Task<List<OrderRequestDTO.ReadOrderListDTO>> GetCustomerOrderHistoryAsync(Guid customerId);

    }
}
