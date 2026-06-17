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
        // VOUCHER — TOGGLE STATUS
        // ============================================================
        Task<Voucher> UpdateAsync(Voucher voucher);
    }
}
