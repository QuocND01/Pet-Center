using AttendanceAPI.DTOs;
using AttendanceAPI.Models;

namespace AttendanceAPI.Repository.Interface
{
    public interface IAttendanceRepository
    {
        Task<IEnumerable<Attendance>> GetAllWithFilterAsync(AttendanceQueryParameters query);
        Task<Attendance?> GetByIdAsync(Guid id);
        Task AddAsync(Attendance attendance);
        void Update(Attendance attendance);
        Task<bool> SaveChangesAsync();
    }
}
