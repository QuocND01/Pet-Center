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
    }
}
