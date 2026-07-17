using PetCenterAPI.Models;

namespace PetCenterAPI.Repository.Interface
{
    public interface IScheduleRepository
    {
        Task<ScheduleException?> GetScheduleExceptionAsync(
        Guid staffId,
        DateOnly date);
        
        Task<GlobalWorkSchedule?> GetGlobalWorkScheduleAsync(
            byte dayOfWeek);
    }
}
