using Microsoft.EntityFrameworkCore;
using Pet_Center.Models;
using Pet_Center.Repository.Interface;

namespace Pet_Center.Repository
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
