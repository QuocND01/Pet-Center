using InventoryAPI.Models;
using InventoryAPI.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace InventoryAPI.Repository
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly PetCenterInventoryServiceContext _context;

        public InventoryRepository(PetCenterInventoryServiceContext context)
        {
            _context = context;
        }
        public async Task<List<Inventory>> GetByProductIds(List<Guid> productIds)
        {
            return await _context.Inventories
                .Where(x => productIds.Contains((Guid)x.ProductId))
                .ToListAsync();
        }
        public IQueryable<Inventory> GetAll()
        {
            return _context.Inventories.AsQueryable();
        }

        public async Task<Inventory?> GetByIdAsync(Guid id)
        {
            return await _context.Inventories.FindAsync(id);
        }
    }
}
