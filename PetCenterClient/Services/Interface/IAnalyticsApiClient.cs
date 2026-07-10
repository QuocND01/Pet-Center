using PetCenterClient.ViewModels;

namespace PetCenterClient.Services.Interface
{
    public interface IAnalyticsApiClient
    {
        Task<DashboardMetricsViewModel> GetDashboardMetricsAsync(string? startDate = null, string? endDate = null);
    }
}