namespace PetCenterAPI.Service.Interface
{
    public interface IStatisticsService
    {
        Task<DashboardStatsDto> GetAdminDashboardStatsAsync(int year);
    }
}
