// File: Repositories/VetProfileRepository.cs
using Microsoft.EntityFrameworkCore;
using StaffAPI.Models;
using StaffAPI.Repositories.Interfaces;

namespace StaffAPI.Repositories;

public class VetProfileRepository : IVetProfileRepository
{
    private readonly PetCenterStaffServiceContext _context;

    public VetProfileRepository(PetCenterStaffServiceContext context)
    {
        _context = context;
    }

    
    public IQueryable<VetProfile> GetAll()
        => _context.VetProfiles.Include(v => v.Staff).AsQueryable();

    
    public async Task<VetProfile?> GetByIdAsync(Guid id)
        => await _context.VetProfiles.Include(v => v.Staff)
            .FirstOrDefaultAsync(v => v.VetProfileId == id);

    
    public async Task<VetProfile?> GetByStaffIdAsync(Guid staffId)
        => await _context.VetProfiles.FirstOrDefaultAsync(v => v.StaffId == staffId);

    
    public async Task<VetProfile?> GetByLicenseNumberAsync(string licenseNumber)
        => await _context.VetProfiles.FirstOrDefaultAsync(v => v.LicenseNumber == licenseNumber);

    
    public async Task<VetProfile> CreateAsync(VetProfile vetProfile)
    {
        _context.VetProfiles.Add(vetProfile);
        await _context.SaveChangesAsync();
        return vetProfile;
    }

    
    public async Task<VetProfile> UpdateAsync(VetProfile vetProfile)
    {
        _context.VetProfiles.Update(vetProfile);
        await _context.SaveChangesAsync();
        return vetProfile;
    }

    
    public async Task SoftDeleteAsync(VetProfile vetProfile)
    {
        vetProfile.IsActive = false;
        _context.VetProfiles.Update(vetProfile);
        await _context.SaveChangesAsync();
    }
}