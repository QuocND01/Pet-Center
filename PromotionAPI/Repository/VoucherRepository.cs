using Microsoft.EntityFrameworkCore;
using PromotionAPI.Models;
using PromotionAPI.Repository.Interface;

namespace PromotionAPI.Repository
{
    public class VoucherRepository : IVoucherRepository
    {
        private readonly VoucherDbContext _context;

        public VoucherRepository(VoucherDbContext context)
        {
            _context = context;
        }

        public async Task<List<Voucher>> GetAllAsync()
            => await _context.Vouchers.ToListAsync();

        public async Task<Voucher?> GetByIdAsync(Guid id)
            => await _context.Vouchers.FindAsync(id);

        public async Task<Voucher?> GetByCodeAsync(string code)
            => await _context.Vouchers.FirstOrDefaultAsync(x => x.Code == code);

        public async Task AddAsync(Voucher v)
        {
            _context.Vouchers.Add(v);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Voucher v)
        {
            _context.Vouchers.Update(v);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var v = await GetByIdAsync(id);
            if (v != null)
            {
                _context.Vouchers.Remove(v);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<CustomerVoucher?> GetCustomerVoucher(Guid customerId, Guid voucherId)
        {
            return await _context.CustomerVouchers
                .FirstOrDefaultAsync(x => x.CustomerId == customerId && x.VoucherId == voucherId);
        }

        public async Task AddCustomerVoucher(CustomerVoucher cv)
        {
            _context.CustomerVouchers.Add(cv);
            await _context.SaveChangesAsync();
        }
    }
}
