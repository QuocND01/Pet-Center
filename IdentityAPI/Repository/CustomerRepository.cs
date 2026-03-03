using IdentityAPI.Models;
using IdentityAPI.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace IdentityAPI.Repository
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly PetCenterContext _context;

        public CustomerRepository(PetCenterContext context)
        {
            _context = context;
        }

        // ==================================== Login ====================================
        public async Task<Customer?> GetByEmailAsync(string email)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(x => x.Email == email && x.IsActive == true);
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
    }
}
