using InventoryAPI.Repository.Interface;
using InventoryAPI.Models;
using System;
using Microsoft.EntityFrameworkCore;
using InventoryAPI.Data;

namespace InventoryAPI.Repository
{
    public class ImportStockRepository : IImportStockRepository
    {
        private readonly InventoryContext _context;

        public ImportStockRepository(InventoryContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ImportStock importStock)
        {
            await _context.ImportStocks.AddAsync(importStock);
        }

        public async Task<ImportStock?> GetByIdAsync(Guid id)
        {
            return await _context.ImportStocks
                .Include(x => x.ImportStockDetails)
                .FirstOrDefaultAsync(x => x.ImportId == id);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
