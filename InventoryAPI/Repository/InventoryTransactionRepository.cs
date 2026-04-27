using InventoryAPI.Models;
using InventoryAPI.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System;

namespace InventoryAPI.Repository
{
    public class InventoryTransactionRepository : IInventoryTransactionRepository
    {
        private readonly PetCenterInventoryServiceContext _context;

        public InventoryTransactionRepository(PetCenterInventoryServiceContext context)
        {
            _context = context;
        }

        public IQueryable<InventoryTransaction> GetAll()
        {
            return _context.InventoryTransactions.AsQueryable();
        }

        public async Task<List<InventoryTransaction>> GetByInventoryIdAsync(Guid inventoryId)
        {
            return await _context.InventoryTransactions
                .Where(x => x.InventoryId == inventoryId)
                .ToListAsync();
        }
    }
}
