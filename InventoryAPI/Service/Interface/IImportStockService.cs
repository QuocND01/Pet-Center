using InventoryAPI.DTOs;

namespace InventoryAPI.Service.Interface
{
    public interface IImportStockService
    {
        Task<Guid> CreateAsync(CreateImportStockDto dto, Guid staffGuid);
        Task<ReadImportStockDto?> GetByIdAsync(Guid id);
        Task<List<IncreaseStockItemDto>> ConfirmAsync(Guid id);
        Task CancelAsync(Guid id);

        Task<List<ReadImportHeaderDto>> GetAllImportsAsync();
        Task<ImportExportResponseDto> Export(DateTime? fromDate, DateTime? toDate);
        Task<string> DeductFIFO(Guid productId, int quantity);
        Task ReturnStock(string mapping);
    }
}
