using Microsoft.EntityFrameworkCore;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;

namespace PetCenterAPI.Repository
{
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly PetCenterContext _context;

        public ScheduleRepository(PetCenterContext context)
        {
            _context = context;
        }
        public async Task<ScheduleException?>
    GetScheduleExceptionAsync(
        Guid staffId,
        DateOnly date)
        {
            return await _context.ScheduleExceptions
                .FirstOrDefaultAsync(x =>
                    x.ExceptionDate == date &&
                    (x.StaffId == null || x.StaffId == staffId));
        }
        public async Task<GlobalWorkSchedule?>
    GetGlobalWorkScheduleAsync(
        byte dayOfWeek)
        {
            return await _context.GlobalWorkSchedules
                .FirstOrDefaultAsync(x =>
                    x.DayOfWeek == dayOfWeek);
        }
    }
}
