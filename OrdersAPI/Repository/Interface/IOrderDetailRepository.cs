using PetCenterAPI.Models;

namespace PetCenterAPI.Repository.Interface
{
    public interface IOrderDetailRepository
    {
        Task<IEnumerable<OrderDetail>> GetByOrderIdAsync(Guid orderId);
        Task<OrderDetail?> GetByIdAsync(Guid id);
        Task AddAsync(OrderDetail detail);
        void Update(OrderDetail detail);
        void Delete(OrderDetail detail);
        Task<bool> SaveChangesAsync();
        Task<List<Guid?>> GetTopSellingProductIds(int months = 3, int top = 10);
        Task<bool> CheckProductInOrdersAsync(Guid productId);


    }
}
