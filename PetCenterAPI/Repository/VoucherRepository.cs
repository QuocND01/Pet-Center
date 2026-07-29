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

        // ============================================================
        // VOUCHER — TOGGLE STATUS
        // ============================================================
        public async Task<Voucher> UpdateAsync(Voucher voucher)
        {
            voucher.Code = voucher.Code.ToUpper().Trim();

            _context.Vouchers.Update(voucher);
            await _context.SaveChangesAsync();

            return voucher;
        }

        // ============================================================
        // CUSTOMER VOUCHER OPERATIONS
        // ============================================================
        public async Task<bool> HasCustomerUsedVoucherAsync(Guid customerId, Guid voucherId)
        {
            return await _context.CustomerVouchers
                .AnyAsync(cv => cv.CustomerId == customerId
                             && cv.VoucherId == voucherId
                             && cv.IsUsed == true);
        }

        public async Task<CustomerVoucher?> GetCustomerVoucherAsync(Guid customerId, Guid voucherId)
        {
            return await _context.CustomerVouchers
                .FirstOrDefaultAsync(cv => cv.CustomerId == customerId
                                     && cv.VoucherId == voucherId);
        }

        public async Task AddCustomerVoucherAsync(CustomerVoucher customerVoucher)
        {
            await _context.CustomerVouchers.AddAsync(customerVoucher);
        }

        public Task UpdateCustomerVoucherAsync(CustomerVoucher customerVoucher)
        {
            _context.CustomerVouchers.Update(customerVoucher);
            return Task.CompletedTask;
        }

        public async Task<List<DTOs.Responses.Order.AvailableVoucherDTO>> GetAvailableVouchersForCustomerAsync(Guid customerId, decimal orderAmount)
        {
            var now = DateTime.Now;

            var usedVoucherIds = await _context.CustomerVouchers
                .Where(cv => cv.CustomerId == customerId && cv.IsUsed == true)
                .Select(cv => cv.VoucherId)
                .ToListAsync();

            return await _context.Vouchers
                .Where(v => v.IsActive == true
                         && (v.ExpiredDate == null || v.ExpiredDate >= now)
                         && (v.MinOrderAmount == null || v.MinOrderAmount <= orderAmount)
                         && (v.UseageLimit == null || v.UseageLimit > 0)
                         && !usedVoucherIds.Contains(v.VoucherId))
                .Select(v => new DTOs.Responses.Order.AvailableVoucherDTO
                {
                    VoucherId = v.VoucherId,
                    Code = v.Code,
                    Description = v.Description,
                    DiscountPercent = v.DiscountPercent,
                    MinOrderAmount = v.MinOrderAmount,
                    MaxDiscountAmount = v.MaxDiscountAmount,
                    ExpiredDate = v.ExpiredDate
                })
                .ToListAsync();
        }
    }
}
