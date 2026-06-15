using Microsoft.EntityFrameworkCore;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;

namespace PetCenterAPI.Repository
{
    public class StaffAuthRepository : IStaffAuthRepository
    {
        private readonly PetCenterContext _context;

        public StaffAuthRepository(PetCenterContext context)
        {
            _context = context;
        }

        // ============================================================
        // LOGIN
        // ============================================================

        /// <summary>
        /// Get staff by email with roles eagerly loaded for login flow
        /// </summary>
        public async Task<Staff?> GetByEmailAsync(string email)
            => await _context.Staffs
                .Include(s => s.Roles)
                .FirstOrDefaultAsync(x => x.Email == email);

        /// <summary>
        /// Get staff by ID with roles, no tracking for read-only use
        /// </summary>
        public async Task<Staff?> GetByIdAsync(Guid staffId)
            => await _context.Staffs
                .Include(s => s.Roles)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.StaffId == staffId);

        /// <summary>
        /// Get all staff with roles, no tracking for read-only use
        /// </summary>
        public async Task<List<Staff>> GetAllAsync()
            => await _context.Staffs
                .Include(s => s.Roles)
                .AsNoTracking()
                .ToListAsync();

        /// <summary>
        /// Persist changes to a staff record
        /// </summary>
        public async Task<bool> UpdateAsync(Staff staff)
        {
            _context.Staffs.Update(staff);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
