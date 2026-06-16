using PetCenterAPI.DTOs.Responses.ManageVoucher;

namespace PetCenterAPI.Service.Interface
{
    public interface IVoucherService
    {
        // ============================================================
        // VOUCHER — VIEW LIST
        // ============================================================
        Task<IEnumerable<VoucherResponseDTO>> GetAllAsync();
    }
}
