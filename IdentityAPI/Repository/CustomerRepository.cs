using IdentityAPI.Models;
using IdentityAPI.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace IdentityAPI.Repository
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly PetCenterIdentityServiceDBContext _context;

        public CustomerRepository(PetCenterIdentityServiceDBContext context)
        {
            _context = context;
        }

        // ==================================== Login ====================================
        public async Task<Customer?> GetByEmailAsync(string email)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(x => x.Email == email && x.IsActive == true);
        }

        public async Task<Customer?> GetByEmailAsyncWithoutActiveCheck(string email)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<bool> DeleteAsync(Customer customer)
        {
            _context.Customers.Remove(customer);
            return await _context.SaveChangesAsync() > 0;
        }

        // ==================================== For Staff and Admin ====================================
        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            return await _context.Customers
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Customer?> GetCustomerByIdAsync(Guid customerId)
        {
            return await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);
        }

        // ==================================== For Customer ====================================
        public async Task<Customer?> GetByIdAsync(Guid customerId)
        {
            return await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);
        }

        public async Task<bool> UpdateAsync(Customer customer)
        {
            _context.Customers.Update(customer);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> AddAsync(Customer customer)
        {
            await _context.Customers.AddAsync(customer);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
