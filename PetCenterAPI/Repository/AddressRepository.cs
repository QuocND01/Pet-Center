using Microsoft.EntityFrameworkCore;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;

namespace PetCenterAPI.Repository
{
    public class AddressRepository : IAddressRepository
    {
        private readonly PetCenterContext _db;

        public AddressRepository(PetCenterContext db)
        {
            _db = db;
        }

        public async Task<List<Address>> GetAddressesByCustomerIdAsync(Guid customerId)
        {
            // Only return active (not soft-deleted) addresses and order default first
            return await _db.Addresses
                .Where(a => a.CustomerId == customerId && a.IsActive == true)
                .OrderByDescending(a => a.IsDefault)
                .ToListAsync();
        }

        public async Task<Address?> GetAddressByIdAsync(Guid addressId, Guid customerId)
        {
            // Only fetch active addresses to avoid operating on soft-deleted records
            return await _db.Addresses
                .FirstOrDefaultAsync(a => a.AddressId == addressId && a.CustomerId == customerId && a.IsActive == true);
        }

        public async Task AddAddressAsync(Address address)
        {
            await _db.Addresses.AddAsync(address);
        }

        public Task UpdateAddressAsync(Address address)
        {
            _db.Addresses.Update(address);
            return Task.CompletedTask;
        }

        public Task DeleteAddressAsync(Address address)
        {
            // Soft-delete: mark the address as inactive instead of removing it from DB
            address.IsActive = false;
            _db.Addresses.Update(address);
            return Task.CompletedTask;
        }

        public async Task ResetDefaultAddressAsync(Guid customerId)
        {
            var defaults = await _db.Addresses
                .Where(a => a.CustomerId == customerId && a.IsDefault == true)
                .ToListAsync();

            foreach (var item in defaults)
            {
                item.IsDefault = false;
            }
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}