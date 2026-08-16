using Microsoft.EntityFrameworkCore;
using PetCenterAPI.DTOs.Requests.Dashboard;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;

namespace PetCenterAPI.Repository
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
            // Kết hợp doanh thu từ Orders (Status == 4) và Appointments (Status == 3)
            var orderMonthly = _db.Orders
                .Where(o => o.OrderDate >= startDate && o.Status == 4)
                .Select(o => new { Year = o.OrderDate.Value.Year, Month = o.OrderDate.Value.Month, Amount = o.TotalAmount });

            var appointmentMonthly = _db.Appointments
                .Where(a => a.AppointmentStart >= startDate && a.Status == 3)
                .Select(a => new { Year = a.AppointmentStart.Year, Month = a.AppointmentStart.Month, Amount = a.PaidAmount });

            return await orderMonthly
                .Concat(appointmentMonthly)
                .GroupBy(x => new { x.Year, x.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new MonthlyRevenueDTO
                {
                    Month = $"{g.Key.Month:D2}/{g.Key.Year}",
                    Revenue = g.Sum(x => x.Amount)
                })
                .ToListAsync();
        }

        public async Task<List<TopProductDTO>> GetTopProductsAsync(int topCount)
        {
            // Count only products from orders that are either Paid (PaymentStatus == 2) or Completed (Status == 4)
            return await _db.OrderDetails
                .Include(od => od.Product)
                .Where(od => od.Order.PaymentStatus == 2 || od.Order.Status == 4)
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
            // Doanh thu gồm cả Orders (Status == 4) và doanh thu từ Appointment (Status == 3)
            var orderRevenue = await _db.Orders
                .Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate && o.Status == 4)
                .SumAsync(o => o.TotalAmount);

            var appointmentRevenue = await _db.Appointments
                .Where(a => a.AppointmentStart >= fromDate && a.AppointmentStart <= toDate && a.Status == 3)
                .SumAsync(a => a.PaidAmount);

            metrics.TotalRevenue = orderRevenue + appointmentRevenue;

            metrics.TotalOrders = await _db.Orders
                .Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate).CountAsync();

            metrics.TotalAppointments = await _db.Appointments
                .Where(a => a.AppointmentStart >= fromDate && a.AppointmentStart <= toDate).CountAsync();

            // Số lượng đã hủy
            metrics.CancelledOrders = await _db.Orders
                .Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate && o.Status == 0)
                .CountAsync();

            metrics.CancelledAppointments = await _db.Appointments
                .Where(a => a.AppointmentStart >= fromDate && a.AppointmentStart <= toDate && a.Status == 0)
                .CountAsync();

            // Tính Gross Profit: lấy tổng giá trị các đơn hàng (đã hoàn thành) trong khoảng
            // trừ đi Giá vốn (COGS) của chính những đơn hàng đó.
            // Lưu ý: metrics.TotalRevenue vẫn bao gồm cả doanh thu appointment; Gross Profit ở đây
            // chỉ tính dựa trên Orders như yêu cầu.
            // 1) Lấy tổng doanh thu đơn hàng (đã tính ở trên: orderRevenue)

            // 2) Tính COGS: từ InventoryTransactions liên quan tới các đơn hàng trong khoảng
            //    (TransactionType = StockOut, ReferenceType = Order) nhân với ImportStockDetail.ImportPrice
            var cogsQuery = from it in _db.InventoryTransactions
                            where it.TransactionType == TransactionType.StockOut && it.ReferenceType == ReferenceType.Order
                            join o in _db.Orders on it.ReferenceId equals o.OrderId
                            where o.OrderDate >= fromDate && o.OrderDate <= toDate
                            join imp in _db.ImportStockDetails on it.ImportStockDetailId equals imp.ImportStockDetailsId into impj
                            from imp in impj.DefaultIfEmpty()
                            select new { Qty = it.QuantityChange, ImportPrice = (decimal?)imp.ImportPrice };

            var cogsList = await cogsQuery.ToListAsync();
            decimal totalCogs = 0m;
            foreach (var item in cogsList)
            {
                var qty = -item.Qty; // QuantityChange is negative for stock out
                var price = item.ImportPrice ?? 0m;
                totalCogs += qty * price;
            }

            // Gross Profit = Orders revenue in period - COGS of those orders
            metrics.TotalProfit = orderRevenue - totalCogs;

            // 2. Biểu đồ Đường: Doanh thu theo từng ngày (kết hợp Orders + Appointments)
            var orderDaily = _db.Orders
                .Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate && o.Status == 4)
                .Select(o => new { Date = o.OrderDate.Value.Date, Total = o.TotalAmount });

            var appointmentDaily = _db.Appointments
                .Where(a => a.AppointmentStart >= fromDate && a.AppointmentStart <= toDate && a.Status == 3)
                .Select(a => new { Date = a.AppointmentStart.Date, Total = a.PaidAmount });

            var rawDaily = await orderDaily.Concat(appointmentDaily)
                .GroupBy(x => x.Date)
                .Select(g => new { Date = g.Key, Total = g.Sum(x => x.Total) })
                .OrderBy(x => x.Date)
                .ToListAsync();

            metrics.RevenueTimeline = rawDaily.Select(x => new ChartItemDTO
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

            // 4. Top Sản phẩm - chỉ tính các sản phẩm trong đơn hàng đã được thanh toán (PaymentStatus == 2)
            // hoặc đơn hàng đã hoàn thành (Status == 4)
            metrics.TopProducts = await _db.OrderDetails
                .Where(od => od.Order.OrderDate >= fromDate && od.Order.OrderDate <= toDate
                             && (od.Order.PaymentStatus == 2 || od.Order.Status == 4))
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