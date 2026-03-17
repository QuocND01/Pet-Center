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

        public async Task AddAsync(ImportStock entity)
        {
            await _context.ImportStocks.AddAsync(entity);
        }

        public async Task<ImportStock?> GetByIdAsync(Guid id)
        {
            return await _context.ImportStocks
                .Include(x => x.Supplier)
                .Include(x => x.ImportStockDetails)
                .FirstOrDefaultAsync(x => x.ImportId == id);
        }

        public async Task<ImportStock?> GetWithDetailsAsync(Guid id)
        {
            return await _context.ImportStocks
                .Include(x => x.Supplier)
                .Include(x => x.ImportStockDetails)
                .FirstOrDefaultAsync(x => x.ImportId == id);
        }

        public async Task<List<ImportStock>> GetAllAsync()
        {
            return await _context.ImportStocks
                .OrderByDescending(x => x.ImportDate).Include(x=>x.Supplier)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task<(List<ImportStock>, List<ImportStockDetail>)> GetExportData(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.ImportStocks
                .Include(x => x.Supplier) 
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(x => x.ImportDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(x => x.ImportDate <= toDate.Value);

            var imports = await query.ToListAsync();

            var importIds = imports.Select(x => x.ImportId).ToList();

            var details = await _context.ImportStockDetails
                .Where(x => x.ImportId.HasValue && importIds.Contains(x.ImportId.Value))
                .ToListAsync();

            return (imports, details);
        }
    }
}
