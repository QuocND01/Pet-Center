using Microsoft.EntityFrameworkCore;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;

namespace PetCenterAPI.Repository
{
    public class InventoryTransactionRepository : IInventoryTransactionRepository
    {
        private readonly PetCenterContext _context;

        public InventoryTransactionRepository(PetCenterContext context)
        {
            _context = context;
        }
        public async Task AddTransactionAsync(
    InventoryTransaction transaction)
        {
            await _context.InventoryTransactions
                .AddAsync(transaction);
        }
        public async Task SaveChange()
        {
            await _context.SaveChangesAsync();
        }
    }
}
