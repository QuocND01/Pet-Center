using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using OrdersAPI.Models;
using OrdersAPI.Repository.Interface;

namespace OrdersAPI.Repository
{
    public class OrderDetailRepository : IOrderDetailRepository
    {
        private readonly PetCenterOrderServiceContext _context;
        public OrderDetailRepository(PetCenterOrderServiceContext context) => _context = context;

        public async Task<IEnumerable<OrderDetail>> GetByOrderIdAsync(Guid orderId) =>
            await _context.OrderDetails.Where(d => d.OrderId == orderId).AsNoTracking().ToListAsync();

        public async Task<OrderDetail?> GetByIdAsync(Guid id) =>
            await _context.OrderDetails.FindAsync(id);

        public async Task AddAsync(OrderDetail detail) => await _context.OrderDetails.AddAsync(detail);
        public void Update(OrderDetail detail) => _context.OrderDetails.Update(detail);
        public void Delete(OrderDetail detail) => _context.OrderDetails.Remove(detail);
        public async Task<bool> SaveChangesAsync() => await _context.SaveChangesAsync() > 0;

        public async Task<List<Guid?>> GetTopSellingProductIds(int months = 3, int top = 10)
        {
            var fromDate = DateTime.UtcNow.AddMonths(-months);

            return new();
        }
    }
}
