using Microsoft.EntityFrameworkCore;
using ProductAPI.Models;
using ProductAPI.Repository.Interface;

namespace ProductAPI.Repository
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
    }
}
