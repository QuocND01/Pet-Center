using PetCenterAPI.DTOs.Requests.Dashboard;

namespace PetCenterAPI.Service.Interface
{
    public interface IAnalyticsService
    {
        Task<DashboardMetricsDTO> GetDashboardDataAsync(DateTime? startDate, DateTime? endDate);
    }
}