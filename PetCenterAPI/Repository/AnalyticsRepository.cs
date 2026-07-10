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

        public async Task<DashboardMetricsDTO> GetDashboardDataAsync(DateTime fromDate, DateTime toDate)
        {
            var metrics = new DashboardMetricsDTO();

            // 1. Chỉ số KPI
            metrics.TotalRevenue = await _db.Orders
                .Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate && o.Status == 4)
                .SumAsync(o => o.TotalAmount);

            metrics.TotalOrders = await _db.Orders
                .Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate).CountAsync();

            metrics.TotalAppointments = await _db.Appointments
                .Where(a => a.AppointmentStart >= fromDate && a.AppointmentStart <= toDate).CountAsync();

            var totalImportCost = await _db.ImportStocks
                .Where(i => i.ImportDate >= fromDate && i.ImportDate <= toDate)
                .SumAsync(i => i.TotalAmount);

            metrics.TotalProfit = metrics.TotalRevenue - totalImportCost; // Lợi nhuận = Doanh thu - Nhập hàng

            // 2. Biểu đồ Đường: Doanh thu theo từng ngày
            var rawRevenue = await _db.Orders
                .Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate && o.Status == 4)
                .GroupBy(o => o.OrderDate.Value.Date)
                .Select(g => new { Date = g.Key, Total = g.Sum(o => o.TotalAmount) })
                .OrderBy(x => x.Date)
                .ToListAsync();

            metrics.RevenueTimeline = rawRevenue.Select(x => new ChartItemDTO
            {
                Label = x.Date.ToString("dd/MM"),
                Value = x.Total
            }).ToList();

            // 3. Biểu đồ Tròn: Tỷ trọng doanh thu theo Danh mục
            metrics.CategoryChart = await _db.OrderDetails
                .Where(od => od.Order.OrderDate >= fromDate && od.Order.OrderDate <= toDate && od.Order.Status == 4)
                .GroupBy(od => od.Product.Category.CategoryName)
                .Select(g => new ChartItemDTO { Label = g.Key, Value = g.Sum(od => od.Quantity * od.UnitPrice) })
                .ToListAsync();

            // 4. Top Sản phẩm
            metrics.TopProducts = await _db.OrderDetails
                .Where(od => od.Order.OrderDate >= fromDate && od.Order.OrderDate <= toDate)
                .GroupBy(od => od.Product.ProductName)
                .Select(g => new ChartItemDTO { Label = g.Key, Value = g.Sum(od => od.Quantity) })
                .OrderByDescending(x => x.Value).Take(5).ToListAsync();

            // 5. Top Dịch vụ thú y
            metrics.TopServices = await _db.AppointmentServices
                .Where(aps => aps.Appointment.AppointmentStart >= fromDate && aps.Appointment.AppointmentStart <= toDate)
                .GroupBy(aps => aps.ServiceName)
                .Select(g => new ChartItemDTO { Label = g.Key, Value = g.Count() })
                .OrderByDescending(x => x.Value).Take(5).ToListAsync();

            return metrics;
        }
    }
}