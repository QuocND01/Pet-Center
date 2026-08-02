using PetCenterAPI.DTOs.Requests.Dashboard;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service.Interface;

namespace PetCenterAPI.Service
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IAnalyticsRepository _repo;

        public AnalyticsService(IAnalyticsRepository repo)
        {
            _repo = repo;
        }

        public async Task<DashboardMetricsDTO> GetDashboardDataAsync(DateTime? startDate, DateTime? endDate)
        {
            // Nếu không truyền ngày, mặc định lấy từ đầu tháng đến hiện tại
            var to = endDate ?? DateTime.Now;
            var from = startDate ?? new DateTime(to.Year, to.Month, 1);

            // Ép giờ để lấy trọn vẹn ngày cuối cùng
            to = to.Date.AddDays(1).AddTicks(-1);
            from = from.Date;

            return await _repo.GetDashboardDataAsync(from, to);

        }
    }
}