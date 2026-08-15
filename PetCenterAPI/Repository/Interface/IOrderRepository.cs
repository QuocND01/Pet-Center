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
        /// <summary>
        /// Truy vấn danh sách đơn hàng của khách kèm thông tin sản phẩm dành riêng cho Rasa Chatbot.
        /// </summary>
        Task<List<Order>> GetOrdersWithItemsByCustomerIdAsync(Guid customerId);

        /// <summary>
        /// Rasa Chatbot: Search customer orders strictly by product name (ProductName).
        /// </summary>
        Task<List<Order>> SearchOrdersByProductNameAsync(Guid customerId, string keyword);
    }
}

