using PetCenterAPI.Models;
using PetCenterAPI.Repositories.Interfaces;
using PetCenterAPI.Services.Interfaces;

namespace PetCenterAPI.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IAnalyticsRepository _repo;

        public AnalyticsService(IAnalyticsRepository repo)
        {
            _repo = repo;
        }

        public async Task<DashboardMetricsDTO> GetDashboardDataAsync()
        {
            var today = DateTime.UtcNow;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var sixMonthsAgo = startOfMonth.AddMonths(-5);

            var metrics = new DashboardMetricsDTO
            {
                TotalRevenue = await _repo.GetTotalRevenueAsync(startOfMonth),
                TotalOrders = await _repo.GetTotalOrdersAsync(startOfMonth),
                TotalAppointments = await _repo.GetTotalAppointmentsAsync(startOfMonth),
                TotalCustomers = await _repo.GetTotalCustomersAsync(),
                TopProducts = await _repo.GetTopProductsAsync(5)
            };

            // Lấy data doanh thu 6 tháng
            var rawMonthlyData = await _repo.GetMonthlyRevenueAsync(sixMonthsAgo);

            // Bơm dữ liệu vào mảng, lấp đầy các tháng bị trống (doanh thu = 0)
            metrics.RevenueChart = new List<MonthlyRevenueDTO>();
            for (int i = 0; i < 6; i++)
            {
                var targetMonth = sixMonthsAgo.AddMonths(i);
                var targetMonthStr = targetMonth.ToString("MM/yyyy");

                var data = rawMonthlyData.FirstOrDefault(m => m.Month == targetMonthStr);

                metrics.RevenueChart.Add(new MonthlyRevenueDTO
                {
                    Month = targetMonthStr,
                    Revenue = data?.Revenue ?? 0
                });
            }

            return metrics;
        }
    }
}