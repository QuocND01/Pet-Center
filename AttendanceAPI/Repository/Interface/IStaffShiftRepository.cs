using AttendanceAPI.DTOs;
using AttendanceAPI.Models;

namespace AttendanceAPI.Repository.Interface
{
    public interface IStaffShiftRepository
    {
        Task<IEnumerable<StaffShift>> GetAllWithFilterAsync(StaffShiftQueryParameters query);
        Task<StaffShift?> GetByIdAsync(Guid id);
        Task AddAsync(StaffShift shift);
        void Update(StaffShift shift);
        Task<bool> SaveChangesAsync();
    }
}
