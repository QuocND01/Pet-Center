using IdentityAPI.Models;
using IdentityAPI.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace IdentityAPI.Repository
{
    public class StaffRepository : IStaffRepository
    {
        private readonly PetCenterContext _context;

        public StaffRepository(PetCenterContext context)
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
            => await _context.Staffs.ToListAsync();

        public async Task<Staff?> GetByIdAsync(Guid id)
            => await _context.Staffs.FindAsync(id);

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
            var staff = await GetByIdAsync(id);
            if (staff != null)
            {
                _context.Staffs.Remove(staff);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> EmailExistsAsync(string email)
            => await _context.Staffs.AnyAsync(s => s.Email == email);
    }
}
