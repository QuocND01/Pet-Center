using Microsoft.EntityFrameworkCore;
using OrdersAPI.Models;
using OrdersAPI.Repository.Interface;

namespace OrdersAPI.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly PetCenterContext _context;
        public OrderRepository(PetCenterContext context) => _context = context;

        public async Task<IEnumerable<Order>> GetAllAsync() =>
            await _context.Orders.Include(o => o.Customer).Include(o => o.Staff).ToListAsync();

        public async Task<Order?> GetByIdAsync(Guid id) =>
            await _context.Orders.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.OrderId == id);

        public async Task AddAsync(Order order) => await _context.Orders.AddAsync(order);
        public void Update(Order order) => _context.Orders.Update(order);
        public void Delete(Order order) => _context.Orders.Remove(order);
        public async Task<bool> SaveChangesAsync() => await _context.SaveChangesAsync() > 0;
    }
}
