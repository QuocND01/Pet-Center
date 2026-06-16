using Microsoft.EntityFrameworkCore;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;

namespace PetCenterAPI.Repository
{
    public class VoucherRepository : IVoucherRepository
    {
        private readonly PetCenterContext _context;

        public VoucherRepository(PetCenterContext context)
        {
            _context = context;
        }

        // ============================================================
        // VOUCHER — VIEW LIST
        // ============================================================
        public async Task<IEnumerable<Voucher>> GetAllAsync()
        {
            return await _context.Vouchers
                .Include(v => v.CustomerVouchers)
                .OrderByDescending(v => v.CreateAt)
                .ToListAsync();
        }
        public async Task<int> GetUsedCountAsync(Guid voucherId)
        {
            return await _context.CustomerVouchers
                .CountAsync(cv => cv.VoucherId == voucherId && cv.IsUsed == true);
        }
    }
}
