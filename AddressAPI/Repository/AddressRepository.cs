using AddressAPI.Models;
using AddressAPI.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace AddressAPI.Repository
{
    public class AddressRepository : IAddressRepository
    {
        private readonly PetCenterIdentityServiceDBContext _context;
        public AddressRepository(PetCenterIdentityServiceDBContext context) => _context = context;

        public async Task<IEnumerable<Address>> GetAllAsync() => await _context.Addresses.ToListAsync();
        public async Task<Address?> GetByIdAsync(Guid id) => await _context.Addresses.FindAsync(id);
        public async Task AddAsync(Address address) => await _context.Addresses.AddAsync(address);
        public void Update(Address address) => _context.Addresses.Update(address);
        public void Delete(Address address) => _context.Addresses.Remove(address);
        public async Task<bool> SaveChangesAsync() => await _context.SaveChangesAsync() > 0;
    }
}
