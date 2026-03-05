using InventoryAPI.Models;
namespace InventoryAPI.Repository.Interface

{
    public interface IImportStockRepository
    {
        Task AddAsync(ImportStock importStock);
        Task<ImportStock?> GetByIdAsync(Guid id);
        Task SaveChangesAsync();
    }
}
