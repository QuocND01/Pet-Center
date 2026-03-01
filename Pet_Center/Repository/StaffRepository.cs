using Microsoft.EntityFrameworkCore;
using Pet_Center.Models;
using Pet_Center.Repository.Interface;

namespace Pet_Center.Repository
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
