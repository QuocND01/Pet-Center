using PetCenterAPI.DTOs.Responses.Order;
using PetCenterAPI.Models;

namespace PetCenterAPI.Repository.Interface
{
    public interface IVoucherRepository
    {
        // ============================================================
        // VOUCHER — VIEW LIST
        // ============================================================
        Task<IEnumerable<Voucher>> GetAllAsync();
        Task<int> GetUsedCountAsync(Guid voucherId);

        // ============================================================
        // VOUCHER — CREATE
        // ============================================================
        Task<Voucher> CreateAsync(Voucher voucher);
        Task<bool> CodeExistsAsync(string code, Guid? excludeId = null);

        // ============================================================
        // VOUCHER — GET BY ID
        // ============================================================
        Task<Voucher?> GetByIdAsync(Guid id);
        
        // ============================================================
        // VOUCHER — TOGGLE STATUS / UPDATE
        // ============================================================
        Task<Voucher> UpdateAsync(Voucher voucher);

        // ============================================================
        // CUSTOMER VOUCHER OPERATIONS
        // ============================================================
        Task<bool> HasCustomerUsedVoucherAsync(Guid customerId, Guid voucherId);
        Task<CustomerVoucher?> GetCustomerVoucherAsync(Guid customerId, Guid voucherId);
        Task AddCustomerVoucherAsync(CustomerVoucher customerVoucher);
        Task UpdateCustomerVoucherAsync(CustomerVoucher customerVoucher);
        Task<List<AvailableVoucherDTO>> GetAvailableVouchersForCustomerAsync(Guid customerId, decimal orderAmount);
    }
}

