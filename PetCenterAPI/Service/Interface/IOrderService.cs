using Microsoft.AspNetCore.OData.Query;
using PetCenterAPI.Common;
using static PetCenterAPI.DTOs.Requests.Order.OrderRequestDTO;
namespace PetCenterAPI.Service.Interface
{
    public interface IOrderService
    {
        Task<List<ReadOrderListDTO>> GetOrderListAdminAsync(ODataQueryOptions<ReadOrderListDTO> queryOptions);

        Task<ReadOrderDetailDTO?> GetOrderDetailsAsync(Guid orderId);

        Task<bool> CancelOrderAsync(Guid orderId);
        Task<int> AdvanceOrderStatusAsync(Guid orderId);
    }
}
