using PetCenterAPI.DTOs.Requests.ManageVoucher;
using PetCenterAPI.DTOs.Responses.ManageVoucher;

namespace PetCenterAPI.Service.Interface
{
    public interface IVoucherService
    {
        // ============================================================
        // VOUCHER — VIEW LIST
        // ============================================================
        Task<IEnumerable<VoucherResponseDTO>> GetAllAsync();

        // ============================================================
        // VOUCHER — CREATE
        // ============================================================
        Task<(bool Success, string Message, VoucherResponseDTO? Data)> CreateAsync(CreateVoucherRequestDTO request);

        // ============================================================
        // VOUCHER — GET BY ID
        // ============================================================
        Task<VoucherResponseDTO?> GetByIdAsync(Guid id);

        // ============================================================
        // VOUCHER — TOGGLE STATUS
        // ============================================================
        Task<(bool Success, string Message)> ToggleStatusAsync(Guid id, bool isActive);
    }
}

