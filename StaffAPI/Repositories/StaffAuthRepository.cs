using Microsoft.EntityFrameworkCore;
using StaffAPI.Models;
using StaffAPI.Repositories.Interfaces;

namespace StaffAPI.Repositories
{
    public class StaffAuthRepository : IStaffAuthRepository
    {
        private readonly PetCenterStaffServiceContext _context;

        public StaffAuthRepository(PetCenterStaffServiceContext context)
        {
            _context = context;
        }

        // Include Roles để JWT generate đúng roles
        public async Task<Staff?> GetByEmailAsync(string email)
            => await _context.Staffs
                .Include(s => s.Roles)
                .FirstOrDefaultAsync(x => x.Email == email);

        public async Task<Staff?> GetByIdAsync(Guid staffId)
            => await _context.Staffs
                .Include(s => s.Roles)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.StaffId == staffId);

        public async Task<List<Staff>> GetAllAsync()
            => await _context.Staffs
                .Include(s => s.Roles)
                .AsNoTracking()
                .ToListAsync();

        public async Task<bool> UpdateAsync(Staff staff)
        {
            _context.Staffs.Update(staff);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
