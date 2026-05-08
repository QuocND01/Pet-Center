using Microsoft.EntityFrameworkCore;
using VoucherAPI.Models;
using VoucherAPI.Repositories.Interfaces;

namespace VoucherAPI.Repositories
{
    public class VoucherRepository : IVoucherRepository
    {
        private readonly PetCenterVoucherServiceContext _context;

        public VoucherRepository(PetCenterVoucherServiceContext context)
        {
            _context = context;
        }

        // ── GET ALL ──────────────────────────────────────────────
        public async Task<IEnumerable<Voucher>> GetAllAsync()
        {
            return await _context.Vouchers
                .Include(v => v.CustomerVouchers)
                .OrderByDescending(v => v.CreateAt)
                .ToListAsync();
        }

        // ── GET BY ID ────────────────────────────────────────────
        public async Task<Voucher?> GetByIdAsync(Guid id)
        {
            return await _context.Vouchers
                .Include(v => v.CustomerVouchers)
                .FirstOrDefaultAsync(v => v.VoucherId == id);
        }

        // ── GET BY CODE ──────────────────────────────────────────
        public async Task<Voucher?> GetByCodeAsync(string code)
        {
            return await _context.Vouchers
                .Include(v => v.CustomerVouchers)
                .FirstOrDefaultAsync(v => v.Code == code.ToUpper());
        }

        // ── CREATE ───────────────────────────────────────────────
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

        // ── UPDATE ───────────────────────────────────────────────
        public async Task<Voucher> UpdateAsync(Voucher voucher)
        {
            voucher.Code = voucher.Code.ToUpper().Trim();
            _context.Vouchers.Update(voucher);
            await _context.SaveChangesAsync();
            return voucher;
        }

        // ── EXISTS ───────────────────────────────────────────────
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Vouchers.AnyAsync(v => v.VoucherId == id);
        }

        // ── CODE EXISTS (dùng khi create/update, bỏ qua chính nó khi edit) ──
        public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null)
        {
            var query = _context.Vouchers.Where(v => v.Code == code.ToUpper());
            if (excludeId.HasValue)
                query = query.Where(v => v.VoucherId != excludeId.Value);
            return await query.AnyAsync();
        }

        // ── GET USED COUNT ───────────────────────────────────────
        public async Task<int> GetUsedCountAsync(Guid voucherId)
        {
            return await _context.CustomerVouchers
                .CountAsync(cv => cv.VoucherId == voucherId && cv.IsUsed == true);
        }
    }
}
