using AttendanceAPI.DTOs;
using AttendanceAPI.Models;
using AttendanceAPI.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace AttendanceAPI.Repository
{
    public class StaffShiftRepository : IStaffShiftRepository
    {
        private readonly PetCenterAttendanceServiceContext _context;

        public StaffShiftRepository(PetCenterAttendanceServiceContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<StaffShift>> GetAllWithFilterAsync(StaffShiftQueryParameters query)
        {
            var shifts = _context.StaffShifts.Include(s => s.Template).AsQueryable();

            if (query.StaffId.HasValue)
                shifts = shifts.Where(s => s.StaffId == query.StaffId.Value);

            if (query.TemplateId.HasValue)
                shifts = shifts.Where(s => s.TemplateId == query.TemplateId.Value);

            // Convert DateTime to DateOnly for comparison
            if (query.FromDate.HasValue)
            {
                var fromDate = DateOnly.FromDateTime(query.FromDate.Value);
                shifts = shifts.Where(s => s.ShiftDate >= fromDate);
            }

            if (query.ToDate.HasValue)
            {
                var toDate = DateOnly.FromDateTime(query.ToDate.Value);
                shifts = shifts.Where(s => s.ShiftDate <= toDate);
            }

            if (query.Status.HasValue)
                shifts = shifts.Where(s => s.Status == query.Status.Value);

            return await shifts
                .OrderByDescending(s => s.ShiftDate)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<StaffShift?> GetByIdAsync(Guid id) =>
            await _context.StaffShifts
                          .Include(s => s.Template)
                          .FirstOrDefaultAsync(s => s.ShiftId == id);

        public async Task AddAsync(StaffShift shift) => await _context.StaffShifts.AddAsync(shift);

        public void Update(StaffShift shift) => _context.StaffShifts.Update(shift);

        public async Task<bool> SaveChangesAsync() => await _context.SaveChangesAsync() > 0;
    }
}