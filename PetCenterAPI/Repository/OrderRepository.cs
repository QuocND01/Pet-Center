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

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}