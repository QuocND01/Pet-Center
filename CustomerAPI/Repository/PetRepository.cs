using CustomerAPI.Models;
using CustomerAPI.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace CustomerAPI.Repository;

public class PetRepository : IPetRepository
{
    private readonly PetCenterCustomerServiceContext _context;

    public PetRepository(PetCenterCustomerServiceContext context)
    {
        _context = context;
    }

    
    public IQueryable<Pet> GetAll()
        => _context.Pets.AsQueryable();

    
    public async Task<Pet?> GetByIdAsync(Guid petId)
        => await _context.Pets.FirstOrDefaultAsync(p => p.PetId == petId);

    
    public async Task<List<Pet>> GetByCustomerIdAsync(Guid customerId)
        => await _context.Pets
            .Where(p => p.CustomerId == customerId)
            .ToListAsync();

    
    public async Task<List<Pet>> SearchAsync(Guid customerId, string keyword)
    {
        var kw = keyword.Trim().ToLower();
        return await _context.Pets
            .Where(p => p.CustomerId == customerId
                     && p.IsActive == true
                     && (p.Species!.ToLower().Contains(kw)
                      || p.Breed!.ToLower().Contains(kw)))
            .ToListAsync();
    }

    
    public async Task<Pet> AddAsync(Pet pet)
    {
        await _context.Pets.AddAsync(pet);
        await _context.SaveChangesAsync();
        return pet;
    }

    
    public async Task<Pet> UpdateAsync(Pet pet)
    {
        _context.Pets.Update(pet);
        await _context.SaveChangesAsync();
        return pet;
    }

    
    public async Task<Pet?> SoftDeleteAsync(Guid petId)
    {
        var pet = await GetByIdAsync(petId);
        if (pet is null) return null;

        pet.IsActive = false;
        await _context.SaveChangesAsync();
        return pet;
    }
}