using InventoryAPI.Models;

namespace InventoryAPI.Repository.Interface
{
    public interface IInventoryRepository
    {
        public Task<List<Inventory>> GetByProductIds(List<Guid> productIds);
        IQueryable<Inventory> GetAll();
        Task<Inventory?> GetByIdAsync(Guid id);
    }
}
