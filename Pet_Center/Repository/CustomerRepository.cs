using Microsoft.EntityFrameworkCore;
using ProductAPI.Models;
using ProductAPI.Repository.Interface;

namespace ProductAPI.Repository
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
    }
}
