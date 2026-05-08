using CustomerAPI.Models;
using CustomerAPI.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System;

namespace CustomerAPI.Repository
{
    public class AddressRepository : IAddressRepository
    {
        private readonly PetCenterCustomerServiceContext _context;

        public AddressRepository(PetCenterCustomerServiceContext context)
        {
            _context = context;
        }

        public async Task<List<Address>> GetByCustomerIdAsync(Guid customerId)
        {
            return await _context.Addresses
                .Where(x => x.CustomerId == customerId && x.IsActive == true)
                .ToListAsync();
        }

        public async Task<Address?> GetByIdAsync(Guid id)
        {
            return await _context.Addresses.FindAsync(id);
        }

        public async Task AddAsync(Address address)
        {
            await _context.Addresses.AddAsync(address);
        }

        public void Update(Address address)
        {
            _context.Addresses.Update(address);
        }

        public void Delete(Address address)
        {
            address.IsActive = false;
            _context.Addresses.Update(address);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
