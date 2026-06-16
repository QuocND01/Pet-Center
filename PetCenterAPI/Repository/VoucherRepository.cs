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

        // ============================================================
        // VOUCHER — CREATE
        // ============================================================
        public async Task<Voucher> CreateAsync(Voucher voucher)
        {
            voucher.VoucherId = Guid.NewGuid();
            voucher.CreateAt = DateTime.UtcNow;
            voucher.IsActive = true;
            voucher.Code = voucher.Code.ToUpper().Trim();

            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();

            return voucher;
        }

        public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null)
        {
            var query = _context.Vouchers.Where(v => v.Code == code.ToUpper());

            if (excludeId.HasValue)
                query = query.Where(v => v.VoucherId != excludeId.Value);

            return await query.AnyAsync();
        }

        // ============================================================
        // VOUCHER — GET BY ID
        // ============================================================
        public async Task<Voucher?> GetByIdAsync(Guid id)
        {
            return await _context.Vouchers
                .FirstOrDefaultAsync(v => v.VoucherId == id);
        }
    }
}
