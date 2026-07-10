using PetCenterAPI.Models;

namespace PetCenterAPI.Repositories.Interfaces
{
    public interface IAnalyticsRepository
    {
        Task<decimal> GetTotalRevenueAsync(DateTime startDate);
        Task<int> GetTotalOrdersAsync(DateTime startDate);
        Task<int> GetTotalAppointmentsAsync(DateTime startDate);
        Task<int> GetTotalCustomersAsync();
        Task<List<MonthlyRevenueDTO>> GetMonthlyRevenueAsync(DateTime startDate);
        Task<List<TopProductDTO>> GetTopProductsAsync(int topCount);
    }
}