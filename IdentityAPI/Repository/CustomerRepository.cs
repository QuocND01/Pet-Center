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

        public async Task<Customer?> GetByEmailAsync(string email)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(x => x.Email == email && x.IsActive == true);
        }

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
    }
}
