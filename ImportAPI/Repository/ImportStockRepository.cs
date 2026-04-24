using ImportAPI.Models;
using System;
using Microsoft.EntityFrameworkCore;

using ImportAPI.Repository.Interface;

namespace ImportAPI.Repository
{
    public class ImportStockRepository : IImportStockRepository
    {
        private readonly PetCenterImportServiceContext _context;

        public ImportStockRepository(PetCenterImportServiceContext context)
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
                .Where(x => importIds.Contains(x.ImportId))
                .ToListAsync();

            return (imports, details);
        }
        public async Task<List<ImportStockDetail>> GetAvailableStock(Guid productId)
        {
            return await _context.ImportStockDetails
                .Include(x => x.Import)
                .Where(x => x.ProductId == productId
                            && x.StockLeft > 0
                            && x.Import!.Status == ImportStock.ImportStatus.Confirmed)
                .OrderBy(x => x.Import!.ImportDate)
                .ToListAsync();
        }

        public async Task<ImportStockDetail?> GetById(Guid id)
        {
            return await _context.ImportStockDetails.FindAsync(id);
        }
    }
}
