using InventoryAPI.Models;

namespace InventoryAPI.Repository.Interface
{
    public interface IInventoryTransactionRepository
    {
        IQueryable<InventoryTransaction> GetAll();
        Task<List<InventoryTransaction>> GetByInventoryIdAsync(Guid inventoryId);
    }
}

