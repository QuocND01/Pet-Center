using Microsoft.EntityFrameworkCore;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;

namespace PetCenterAPI.Repository
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly PetCenterContext _context;

        public CustomerRepository(PetCenterContext context)
        {
            _context = context;
        }

        // ============================================================
        // LOGIN
        // ============================================================

        /// <summary>
        /// Get customer by email, only returns active accounts
        /// </summary>
        public async Task<Customer?> GetByEmailAsync(string email)
            => await _context.Customers
                .FirstOrDefaultAsync(x => x.Email == email && x.IsActive == true);

        /// <summary>
        /// Get customer by email regardless of active status, used for login flow
        /// </summary>
        public async Task<Customer?> GetByEmailAsyncWithoutActiveCheck(string email)
            => await _context.Customers
                .FirstOrDefaultAsync(x => x.Email == email);
    }
}
