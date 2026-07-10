using PetCenterClient.ViewModels;

namespace PetCenterClient.Services.Interface
{
    public interface IAnalyticsApiClient
    {
        Task<DashboardMetricsViewModel> GetDashboardMetricsAsync();
    }
}