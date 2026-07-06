using Microsoft.EntityFrameworkCore;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;

namespace PetCenterAPI.Repository
{
    public class PetRepository : IPetRepository
    {
        private readonly PetCenterContext _db;
        public PetRepository(PetCenterContext db) => _db = db;

        public async Task<List<Pet>> GetPetsByCustomerIdAsync(Guid customerId)
        {
            return await _db.Pets
                .Where(p => p.CustomerId == customerId && p.IsActive == true)
                .OrderByDescending(p => p.Species)
                .ToListAsync();
        }

        public async Task<Pet?> GetPetByIdAsync(Guid petId, Guid customerId)
        {
            return await _db.Pets
                .FirstOrDefaultAsync(p => p.PetId == petId && p.CustomerId == customerId && p.IsActive == true);
        }
    }
}