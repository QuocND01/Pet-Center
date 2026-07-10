using PetCenterAPI.Models;

namespace PetCenterAPI.Services.Interfaces
{
    public interface IAnalyticsService
    {
        Task<DashboardMetricsDTO> GetDashboardDataAsync(DateTime? startDate, DateTime? endDate);
    }
}