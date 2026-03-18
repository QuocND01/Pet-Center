namespace OrdersAPI.Service.Interface
{
    public interface IStatisticsService
    {
        Task<DashboardStatsDto> GetAdminDashboardStatsAsync(int year);
    }
}
