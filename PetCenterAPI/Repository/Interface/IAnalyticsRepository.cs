using PetCenterAPI.DTOs.Requests.Dashboard;

namespace PetCenterAPI.Repository.Interface
{
    public interface IAnalyticsRepository
    {
        Task<decimal> GetTotalRevenueAsync(DateTime startDate);
        Task<int> GetTotalOrdersAsync(DateTime startDate);
        Task<int> GetTotalAppointmentsAsync(DateTime startDate);
        Task<int> GetTotalCustomersAsync();
        Task<List<MonthlyRevenueDTO>> GetMonthlyRevenueAsync(DateTime startDate);
        Task<List<TopProductDTO>> GetTopProductsAsync(int topCount);
        Task<DashboardMetricsDTO> GetDashboardDataAsync(DateTime fromDate, DateTime toDate);
    }
}