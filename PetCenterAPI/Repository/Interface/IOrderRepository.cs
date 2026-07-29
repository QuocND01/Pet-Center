using Microsoft.EntityFrameworkCore.Storage;
using PetCenterAPI.Models;

namespace PetCenterAPI.Repository.Interface
{
    public interface IOrderRepository
    {
        IQueryable<Order> GetAllOrders();
        Task<Order?> GetOrderByIdAsync(Guid orderId);
        Task<Order?> GetOrderWithDetailsByIdAsync(Guid orderId);
        Task AddOrderAsync(Order order);
        Task AddOrderDetailsAsync(IEnumerable<OrderDetail> details);
        Task AddOrderProductSnapshotsAsync(IEnumerable<OrderProductSnapshot> snapshots);
        Task UpdateOrderAsync(Order order);
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task SaveAsync();
        Task SaveChangesAsync();
        Task<List<Order>> GetOrdersByCustomerIdAsync(Guid customerId);
    }
}

