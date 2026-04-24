// File: Repositories/StaffRepository.cs
using Microsoft.EntityFrameworkCore;
using StaffAPI.Models;
using StaffAPI.Repositories.Interfaces;

namespace StaffAPI.Repositories;

public class StaffRepository : IStaffRepository
{
    private readonly PetCenterStaffServiceContext _context;

    public StaffRepository(PetCenterStaffServiceContext context)
    {
        _context = context;
    }

    
    public IQueryable<Staff> GetAll()
        => _context.Staffs
            .Include(s => s.Roles)
            .Include(s => s.VetProfile)
            .AsQueryable();

    
    public async Task<Staff?> GetByIdAsync(Guid id)
        => await _context.Staffs
            .Include(s => s.Roles)
            .Include(s => s.VetProfile)
            .FirstOrDefaultAsync(s => s.StaffId == id);

    
    public async Task<Staff?> GetByEmailAsync(string email)
        => await _context.Staffs.FirstOrDefaultAsync(s => s.Email == email);

    
    public async Task<Staff> CreateAsync(Staff staff)
    {
        _context.Staffs.Add(staff);
        await _context.SaveChangesAsync();
        return staff;
    }

    
    public async Task<Staff> UpdateAsync(Staff staff)
    {
        _context.Staffs.Update(staff);
        await _context.SaveChangesAsync();
        return staff;
    }

    
    public async Task SoftDeleteAsync(Staff staff)
    {
        staff.IsActive = false;
        _context.Staffs.Update(staff);
        await _context.SaveChangesAsync();
    }

    
    public async Task<bool> ExistsAsync(Guid id)
        => await _context.Staffs.AnyAsync(s => s.StaffId == id);

    
    public async Task<List<Role>> GetRolesByIdsAsync(List<Guid> roleIds)
        => await _context.Roles
            .Where(r => roleIds.Contains(r.RoleId) && r.IsActive)
            .ToListAsync();
}