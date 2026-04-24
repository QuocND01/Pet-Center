using ImportAPI.Models;
namespace ImportAPI.Repository.Interface

{
    public interface IImportStockRepository
    {
        Task AddAsync(ImportStock entity);
        Task<ImportStock?> GetByIdAsync(Guid id);
        Task<ImportStock?> GetWithDetailsAsync(Guid id);
        Task<List<ImportStock>> GetAllAsync();
        Task SaveChangesAsync();
        Task<(List<ImportStock>, List<ImportStockDetail>)> GetExportData(DateTime? fromDate, DateTime? toDate);
        Task<List<ImportStockDetail>> GetAvailableStock(Guid productId);

        //for deduce
        Task<ImportStockDetail?> GetById(Guid id);
        
    }
}
