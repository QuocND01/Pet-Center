using Microsoft.EntityFrameworkCore;
using OrdersAPI.Models;
using OrdersAPI.Repository.Interface;

namespace OrdersAPI.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly PetCenterOrderServiceContext _context;
        public OrderRepository(PetCenterOrderServiceContext context) => _context = context;

        public async Task<IEnumerable<Order>> GetAllAsync() =>
            await _context.Orders.AsNoTracking().ToListAsync();
        public async Task<Order?> GetByIdAsync(Guid id) =>
            await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderId == id);

        public async Task AddAsync(Order order) => await _context.Orders.AddAsync(order);
        public void Update(Order order) => _context.Orders.Update(order);
        public void Delete(Order order) => _context.Orders.Remove(order);
        public async Task<bool> SaveChangesAsync() => await _context.SaveChangesAsync() > 0;

        // Đếm tổng số đơn hàng (không tính đơn đã hủy Status = 0)
        public async Task<int> GetTotalOrdersAsync(int year) =>
            await _context.Orders.Where(o => o.Status != 0 && o.OrderDate.Value.Year == year).CountAsync();

        // Đếm số đơn đang chờ xử lý (Pending: Status = 1)
        public async Task<int> GetPendingOrdersAsync(int year) =>
            await _context.Orders.Where(o => o.Status == 1 && o.OrderDate.Value.Year == year).CountAsync();

        // TỔNG DOANH THU: CHỈ TÍNH CÁC ĐƠN ĐÃ GIAO THÀNH CÔNG (Completed: Status = 4)
        public async Task<decimal> GetTotalRevenueAsync(int year) =>
            await _context.Orders
                .Where(o => o.Status == 4 && o.OrderDate.Value.Year == year)
                .SumAsync(o => o.TotalAmount);

        // BIỂU ĐỒ DOANH THU THEO THÁNG: CHỈ TÍNH ĐƠN HOÀN THÀNH (Status = 4)
        public async Task<List<MonthlyRevenueDto>> GetRevenueByMonthAsync(int year) =>
            await _context.Orders
                .Where(o => o.Status == 4 && o.OrderDate.Value.Year == year)
                .GroupBy(o => o.OrderDate.Value.Month)
                .Select(g => new MonthlyRevenueDto
                {
                    Month = g.Key,
                    Revenue = g.Sum(o => o.TotalAmount)
                })
                .OrderBy(x => x.Month)
                .ToListAsync();
    }
}