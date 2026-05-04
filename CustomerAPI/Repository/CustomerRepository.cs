using CustomerAPI.Models;
using CustomerAPI.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace CustomerAPI.Repository
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly PetCenterCustomerServiceContext _context;

        public CustomerRepository(PetCenterCustomerServiceContext context)
        {
            _context = context;
        }

        // ── Login ──────────────────────────────────────────────
        public async Task<Customer?> GetByEmailAsync(string email)
            => await _context.Customers
                .FirstOrDefaultAsync(x => x.Email == email && x.IsActive == true);

        public async Task<Customer?> GetByEmailAsyncWithoutActiveCheck(string email)
            => await _context.Customers
                .FirstOrDefaultAsync(x => x.Email == email);

        // ── Register ───────────────────────────────────────────
        public async Task<bool> AddAsync(Customer customer)
        {
            await _context.Customers.AddAsync(customer);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(Customer customer)
        {
            _context.Customers.Remove(customer);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Customer?> GetByPhoneAsync(string phone)
            => await _context.Customers
                .FirstOrDefaultAsync(x => x.PhoneNumber == phone && x.IsVerified == true);

        // ── For Staff/Admin ────────────────────────────────────
        public async Task<List<Customer>> GetAllCustomersAsync()
            => await _context.Customers.AsNoTracking().ToListAsync();

        public async Task<Customer?> GetCustomerByIdAsync(Guid customerId)
            => await _context.Customers.AsNoTracking()
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

        // ── For Customer ───────────────────────────────────────
        public async Task<Customer?> GetByIdAsync(Guid customerId)
            => await _context.Customers.AsNoTracking()
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

        public async Task<bool> UpdateAsync(Customer customer)
        {
            _context.Customers.Update(customer);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<OtpCode?> GetOtpByCustomerIdAsync(Guid customerId)
            => await _context.OtpCodes
                .FirstOrDefaultAsync(o => o.CustomerId == customerId);

        public async Task<bool> AddOtpAsync(OtpCode otp)
        {
            await _context.OtpCodes.AddAsync(otp);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateOtpAsync(OtpCode otp)
        {
            _context.OtpCodes.Update(otp);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteOtpAsync(OtpCode otp)
        {
            _context.OtpCodes.Remove(otp);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Customer?> GetByIdInternalAsync(Guid customerId)
    => await _context.Customers.AsNoTracking()
        .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.IsActive == true);
    }
}
