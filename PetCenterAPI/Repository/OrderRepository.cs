using Microsoft.EntityFrameworkCore;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;

namespace PetCenterAPI.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly PetCenterContext _db;

        public OrderRepository(PetCenterContext db)
        {
            _db = db;
        }

        public IQueryable<Order> GetAllOrders()
        {
            // Trả về IQueryable thuần, không thực thi ToList() ở đây
            return _db.Orders.AsQueryable();
        }

        public async Task<Order?> GetOrderByIdAsync(Guid orderId)
        {
            return await _db.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.OrderProductSnapshot)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<Order?> GetOrderWithDetailsByIdAsync(Guid orderId)
        {
            return await _db.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task AddOrderAsync(Order order)
        {
            await _db.Orders.AddAsync(order);
        }

        public async Task AddOrderDetailsAsync(IEnumerable<OrderDetail> details)
        {
            await _db.OrderDetails.AddRangeAsync(details);
        }

        public async Task AddOrderProductSnapshotsAsync(IEnumerable<OrderProductSnapshot> snapshots)
        {
            await _db.OrderProductSnapshots.AddRangeAsync(snapshots);
        }

        public Task UpdateOrderAsync(Order order)
        {
            _db.Orders.Update(order);
            return Task.CompletedTask;
        }

        public async Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync()
        {
            return await _db.Database.BeginTransactionAsync();
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        public async Task<List<Order>> GetOrdersByCustomerIdAsync(Guid customerId)
        {
            return await _db.Orders
                .Include(o => o.Customer)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        /// <summary>
        /// Truy vấn danh sách đơn hàng của khách hàng kèm danh sách sản phẩm (Dành riêng cho Rasa Chatbot).
        /// </summary>
        public async Task<List<Order>> GetOrdersWithItemsByCustomerIdAsync(Guid customerId)
        {
            return await _db.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Product)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }
    }
}