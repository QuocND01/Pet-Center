using Microsoft.EntityFrameworkCore;
using PetCenterAPI.Models;
using PetCenterAPI.Repositories.Interfaces;

namespace PetCenterAPI.Repositories
{
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private readonly PetCenterContext _db;

        public AnalyticsRepository(PetCenterContext db)
        {
            _db = db;
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime startDate)
        {
            return await _db.Orders
                .Where(o => o.OrderDate >= startDate && o.Status == 4) // Status 4 = Hoàn thành
                .SumAsync(o => o.TotalAmount);
        }

        public async Task<int> GetTotalOrdersAsync(DateTime startDate)
        {
            return await _db.Orders
                .Where(o => o.OrderDate >= startDate)
                .CountAsync();
        }

        public async Task<int> GetTotalAppointmentsAsync(DateTime startDate)
        {
            return await _db.Appointments
                .Where(a => a.AppointmentStart >= startDate)
                .CountAsync();
        }

        public async Task<int> GetTotalCustomersAsync()
        {
            return await _db.Customers.CountAsync();
        }

        public async Task<List<MonthlyRevenueDTO>> GetMonthlyRevenueAsync(DateTime startDate)
        {
            return await _db.Orders
                .Where(o => o.OrderDate >= startDate && o.Status == 4)
                .GroupBy(o => new { o.OrderDate.Value.Year, o.OrderDate.Value.Month })
                // 👉 FIX: Chuyển OrderBy lên trước khi Select để không cần dùng biến phụ
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new MonthlyRevenueDTO
                {
                    Month = $"{g.Key.Month:D2}/{g.Key.Year}", // Format MM/yyyy
                    Revenue = g.Sum(o => o.TotalAmount)
                })
                .ToListAsync();
        }

        public async Task<List<TopProductDTO>> GetTopProductsAsync(int topCount)
        {
            return await _db.OrderDetails
                .Include(od => od.Product)
                .GroupBy(od => od.ProductId)
                .Select(g => new TopProductDTO
                {
                    ProductName = g.First().Product.ProductName,
                    TotalSold = g.Sum(od => od.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(topCount)
                .ToListAsync();
        }
    }
}