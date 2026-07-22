using PetCenterClient.DTOs;
using PetCenterClient.ViewModels.ManageVoucher;

namespace PetCenterClient.Services.Interface
{
    public interface IVoucherApiService
    {
        // ============================================================
        // VOUCHER — VIEW LIST
        // ============================================================
        Task<List<VoucherViewModel>> GetAllAsync();
        Task<VoucherViewModel?> GetByIdAsync(Guid id);

        // ============================================================
        // VOUCHER — CREATE
        // ============================================================
        Task<(bool Success, string Message, VoucherViewModel? Data)> CreateAsync(CreateVoucherViewModel dto);

        // ============================================================
        // VOUCHER — TOGGLE STATUS
        // ============================================================
        Task<(bool Success, string Message)> ToggleStatusAsync(Guid id, bool isActive);
    }
}
