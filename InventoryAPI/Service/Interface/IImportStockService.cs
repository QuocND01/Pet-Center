using InventoryAPI.DTOs;

namespace InventoryAPI.Service.Interface
{
    public interface IImportStockService
    {
        Task<Guid> CreateAsync(CreateImportStockDto dto);
        Task<ReadImportStockDto?> GetByIdAsync(Guid id);
        Task ConfirmAsync(Guid id);
        Task CancelAsync(Guid id);

        Task<List<ReadImportHeaderDto>> GetAllImportsAsync();
    }
}
