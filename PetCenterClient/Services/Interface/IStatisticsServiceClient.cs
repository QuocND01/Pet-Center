using PetCenterClient.DTOs;

namespace PetCenterClient.Services.Interface
{
    public interface IStatisticsServiceClient
    {
        Task<DashboardStatsDto?> GetDashboardStatsAsync(int? year);
    }
}