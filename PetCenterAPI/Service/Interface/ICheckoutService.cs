using PetCenterAPI.DTOs.Requests.Order;
using PetCenterAPI.DTOs.Responses.Order;

namespace PetCenterAPI.Service.Interface
{
    public interface ICheckoutService
    {
        Task<PlaceOrderResponseDTO> PlaceCodOrderAsync(PlaceCodOrderDTO dto);
Task<List<AvailableVoucherDTO>> GetAvailableVouchersAsync(Guid customerId, decimal orderAmount);
    }
}
