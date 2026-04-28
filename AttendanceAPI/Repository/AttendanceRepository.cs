using AttendanceAPI.DTOs;
using AttendanceAPI.Models;
using AttendanceAPI.Repository.Interface;
using Microsoft.EntityFrameworkCore;
namespace AttendanceAPI.Repository
{
    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly PetCenterAttendanceServiceContext _context;

        public AttendanceRepository(PetCenterAttendanceServiceContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Attendance>> GetAllWithFilterAsync(AttendanceQueryParameters query)
        {
            var attendances = _context.Attendances.AsQueryable();

            // Lọc theo nhân viên (Chức năng: Search / View History for Staff)
            if (query.StaffId.HasValue)
                attendances = attendances.Where(a => a.StaffId == query.StaffId.Value);

            // Lọc theo ca làm việc
            if (query.ShiftId.HasValue)
                attendances = attendances.Where(a => a.ShiftId == query.ShiftId.Value);

            // Lọc theo trạng thái
            if (query.Status.HasValue)
                attendances = attendances.Where(a => a.Status == query.Status.Value);

            // Lọc theo khoảng thời gian CheckIn
            if (query.FromDate.HasValue)
                attendances = attendances.Where(a => a.CheckInTime >= query.FromDate.Value);

            if (query.ToDate.HasValue)
                attendances = attendances.Where(a => a.CheckInTime <= query.ToDate.Value);

            return await attendances
                .OrderByDescending(a => a.CreatedAt)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Attendance?> GetByIdAsync(Guid id) =>
            await _context.Attendances.FindAsync(id);

        public async Task AddAsync(Attendance attendance) => await _context.Attendances.AddAsync(attendance);

        public void Update(Attendance attendance) => _context.Attendances.Update(attendance);

        public async Task<bool> SaveChangesAsync() => await _context.SaveChangesAsync() > 0;
    }
}