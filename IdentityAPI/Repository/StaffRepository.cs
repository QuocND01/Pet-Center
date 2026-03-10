using IdentityAPI.Models;
using IdentityAPI.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace IdentityAPI.Repository
{
    public class StaffRepository : IStaffRepository
    {
        private readonly PetCenterIdentityServiceDBContext _context;

        public StaffRepository(PetCenterIdentityServiceDBContext context)
        {
            _context = context;
        }

        public async Task<Staff?> GetByEmailAsync(string email)
        {
            return await _context.Staffs
                .Include(s => s.Roles)
                .FirstOrDefaultAsync(s => s.Email == email);
        }

        public async Task<IEnumerable<Staff>> GetAllAsync()
        {
            return await _context.Staffs
                .Include(s => s.Roles)
                .Where(s => s.Roles.Any(r => r.RoleName == "Staff"))  // ← chỉ lấy role Staff
                .ToListAsync();
        }

        public async Task<Staff?> GetByIdAsync(Guid id)
            => await _context.Staffs.Include(s => s.Roles).FirstOrDefaultAsync(s => s.StaffId == id);

        public async Task<IEnumerable<Staff>> SearchAsync(string keyword)
            => await _context.Staffs
                .Where(s => s.FullName.Contains(keyword) || s.Email.Contains(keyword) || s.PhoneNumber.Contains(keyword))
                .ToListAsync();

        public async Task AddAsync(Staff staff)
        {
            await _context.Staffs.AddAsync(staff);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Staff staff)
        {
            _context.Staffs.Update(staff);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var staff = await _context.Staffs
                .Include(s => s.Roles)   // ← Load cả Roles
                .FirstOrDefaultAsync(s => s.StaffId == id);

            if (staff != null)
            {
                // 1. Xóa các bản ghi trong StaffRoles trước
                staff.Roles.Clear();
                await _context.SaveChangesAsync();

                // 2. Sau đó mới xóa Staff
                _context.Staffs.Remove(staff);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> EmailExistsAsync(string email)
            => await _context.Staffs.AnyAsync(s => s.Email == email);

        // ===== Role Management =====

        public async Task<Role?> GetRoleByNameAsync(string roleName)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == roleName && r.IsActive == true);
        }

        public async Task AssignRoleAsync(Guid staffId, Guid roleId)
        {
            // Kiểm tra đã tồn tại chưa để tránh trùng lặp
            var alreadyExists = await _context.Database
                .ExecuteSqlRawAsync(
                    "IF NOT EXISTS (SELECT 1 FROM StaffRoles WHERE StaffID = {0} AND RoleID = {1}) INSERT INTO StaffRoles (StaffID, RoleID) VALUES ({0}, {1})",
                    staffId, roleId);
        }

        public async Task<IEnumerable<Role>> GetRolesByStaffIdAsync(Guid staffId)
        {
            var staff = await _context.Staffs
                .Include(s => s.Roles)
                .FirstOrDefaultAsync(s => s.StaffId == staffId);

            return staff?.Roles ?? new List<Role>();
        }
    }
}