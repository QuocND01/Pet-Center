using InventoryAPI.Models;
using InventoryAPI.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace InventoryAPI.Repository
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly PetCenterInventoryServiceContext _db;

        public InventoryRepository(PetCenterInventoryServiceContext db)
        {
            _db = db;
        }
        public async Task<List<Inventory>> GetByProductIds(List<Guid> productIds)
        {
            return await _db.Inventories
                .Where(x => productIds.Contains((Guid)x.ProductId))
                .ToListAsync();
        }
    }
}
