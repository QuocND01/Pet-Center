using Microsoft.EntityFrameworkCore;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;

namespace PetCenterAPI.Repository
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly PetCenterContext _db;

        public InventoryRepository(PetCenterContext db)
        {
            _db = db;
        }

        public async Task<Inventory?> GetInventoryByProductIdAsync(Guid productId)
        {
            // Truy vấn database để lấy ra thông tin kho của sản phẩm
            return await _db.Inventories.FirstOrDefaultAsync(i => i.ProductId == productId);
        }
    }
}