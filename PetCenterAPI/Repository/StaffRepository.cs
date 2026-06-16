using Microsoft.EntityFrameworkCore;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;

namespace PetCenterAPI.Repository
{
    public class StaffRepository : IStaffRepository
    {
        private readonly PetCenterContext _context;

        /// <summary>Role names that can be assigned to a staff member (Admin excluded).</summary>
        private static readonly string[] AssignableRoleNames =
            { "Sales", "Inventories", "Veterinarian", "Groomer" };

        public StaffRepository(PetCenterContext context)
        {
            _context = context;
        }

        // ============================================================
        // STAFF — VIEW LIST
        // ============================================================
        public async Task<List<Staff>> GetAllAsync()
        {
            return await _context.Staffs
                .Include(s => s.Roles)
                .Include(s => s.VetProfile)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        // ============================================================
        // STAFF — GET BY ID
        // ============================================================
        public async Task<Staff?> GetByIdAsync(Guid staffId)
        {
            return await _context.Staffs
                .Include(s => s.Roles)
                .Include(s => s.VetProfile)
                .FirstOrDefaultAsync(s => s.StaffId == staffId);
        }

        // ============================================================
        // UNIQUENESS CHECKS
        // ============================================================
        public async Task<bool> EmailExistsAsync(string email, Guid? excludeStaffId = null)
        {
            var query = _context.Staffs.Where(s => s.Email == email);
            if (excludeStaffId.HasValue)
                query = query.Where(s => s.StaffId != excludeStaffId.Value);
            return await query.AnyAsync();
        }

        public async Task<bool> PhoneExistsAsync(string phoneNumber, Guid? excludeStaffId = null)
        {
            var query = _context.Staffs.Where(s => s.PhoneNumber == phoneNumber);
            if (excludeStaffId.HasValue)
                query = query.Where(s => s.StaffId != excludeStaffId.Value);
            return await query.AnyAsync();
        }

        public async Task<bool> LicenseNumberExistsAsync(string licenseNumber, Guid? excludeStaffId = null)
        {
            var query = _context.VetProfiles.Where(v => v.LicenseNumber == licenseNumber);
            if (excludeStaffId.HasValue)
                query = query.Where(v => v.StaffId != excludeStaffId.Value);
            return await query.AnyAsync();
        }

        // ============================================================
        // ROLES
        // ============================================================
        public async Task<List<Role>> GetAssignableRolesAsync()
        {
            return await _context.Roles
                .Where(r => r.IsActive && AssignableRoleNames.Contains(r.RoleName))
                .OrderBy(r => r.RoleName)
                .ToListAsync();
        }

        public async Task<Role?> GetRoleByIdAsync(Guid roleId)
        {
            return await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId);
        }

        // ============================================================
        // VET RATING (computed, read-only)
        // ============================================================
        public async Task<decimal> GetVetAverageRatingAsync(Guid staffId)
        {
            var ratings = await _context.VetFeedbacks
                .Where(f => f.StaffId == staffId && f.Status == 1 && f.Star != null)
                .Select(f => (decimal)f.Star!.Value)
                .ToListAsync();

            if (ratings.Count == 0) return 0m;

            return Math.Round(ratings.Average(), 1);
        }

        // ============================================================
        // CREATE
        // ============================================================
        public async Task<Staff> CreateAsync(Staff staff)
        {
            _context.Staffs.Add(staff);
            await _context.SaveChangesAsync();
            return staff;
        }

        // ============================================================
        // UPDATE
        // ============================================================
        public async Task UpdateAsync(Staff staff)
        {
            _context.Staffs.Update(staff);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
