using OrdersAPI.Models;

namespace OrdersAPI.Repository.Interface
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllAsync();
        Task<Order?> GetByIdAsync(Guid id);
        Task AddAsync(Order order);
        void Update(Order order);
        void Delete(Order order);
        Task<bool> SaveChangesAsync();
        Task<decimal> GetTotalRevenueAsync(int year);
        Task<int> GetTotalOrdersAsync(int year);
        Task<int> GetPendingOrdersAsync(int year);
        Task<List<MonthlyRevenueDto>> GetRevenueByMonthAsync(int year);

    }
}
