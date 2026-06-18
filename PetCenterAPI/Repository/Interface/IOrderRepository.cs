using PetCenterAPI.Models;

namespace PetCenterAPI.Repository.Interface
{
    public interface IOrderRepository
    {
        IQueryable<Order> GetAllOrders();
        Task<Order?> GetOrderByIdAsync(Guid orderId);
        Task SaveAsync();
        Task<List<Order>> GetOrdersByCustomerIdAsync(Guid customerId);
    }
}
