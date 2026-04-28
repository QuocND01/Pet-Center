using AttendanceAPI.DTOs;
using AttendanceAPI.Models;

namespace AttendanceAPI.Repository.Interface
{
    public interface IShiftTemplateRepository
    {
        Task<IEnumerable<ShiftTemplate>> GetAllWithFilterAsync(ShiftTemplateQueryParameters query);
        Task<ShiftTemplate?> GetByIdAsync(Guid id);
        Task AddAsync(ShiftTemplate template);
        void Update(ShiftTemplate template);
        Task<bool> SaveChangesAsync();
    }
}
