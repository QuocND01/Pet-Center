using InventoryAPI.DTOs;

namespace InventoryAPI.Service.Interface
{
    public interface IImportStockService
    {
        Task<Guid> CreateAsync(CreateImportStockDto dto);
        Task<ImportStockDto?> GetByIdAsync(Guid id);
        Task ConfirmAsync(Guid id);
        Task CancelAsync(Guid id);
    }
}
