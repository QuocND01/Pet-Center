using OrdersAPI.DTOs;

namespace OrdersAPI.Service.Interface
{
    public interface ICheckoutService
    {
        Task<CheckoutResponseDTO> ProcessCheckoutAsync(CheckoutRequestDTO dto);
        Task<List<CustomerVoucherDTO>> GetAvailableVouchersAsync(Guid customerId, decimal orderAmount);
        Task<VoucherValidateResponseDTO> ValidateVoucherAsync(VoucherValidateRequestDTO dto);
    }
}