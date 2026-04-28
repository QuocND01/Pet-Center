using AttendanceAPI.DTOs;
using AttendanceAPI.Models;
using AttendanceAPI.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace AttendanceAPI.Repository
{
    public class ShiftTemplateRepository : IShiftTemplateRepository
    {
        private readonly PetCenterAttendanceServiceContext _context;

        public ShiftTemplateRepository(PetCenterAttendanceServiceContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ShiftTemplate>> GetAllWithFilterAsync(ShiftTemplateQueryParameters query)
        {
            var templates = _context.ShiftTemplates.AsQueryable();

            if (query.RoleId.HasValue)
                templates = templates.Where(t => t.RoleId == query.RoleId.Value);

            if (query.IsActive.HasValue)
                templates = templates.Where(t => t.IsActive == query.IsActive.Value);

            return await templates
                .OrderByDescending(t => t.CreatedAt)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ShiftTemplate?> GetByIdAsync(Guid id) =>
            await _context.ShiftTemplates.FindAsync(id);

        public async Task AddAsync(ShiftTemplate template) => await _context.ShiftTemplates.AddAsync(template);

        public void Update(ShiftTemplate template) => _context.ShiftTemplates.Update(template);

        public async Task<bool> SaveChangesAsync() => await _context.SaveChangesAsync() > 0;
    }
}