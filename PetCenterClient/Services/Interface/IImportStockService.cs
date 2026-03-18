using PetCenterClient.DTOs;

namespace PetCenterClient.Services.Interface
{
    public interface IImportStockService
    {
        Task<List<ReadImportHeaderDto>> GetAllAsync();
        Task<ReadImportStockDetailDto?> GetByIdAsync(Guid id);
        Task<Guid> CreateAsync(CreateImportStockDto dto);
        Task ConfirmAsync(Guid id);
        Task CancelAsync(Guid id);

        Task<List<ImportDto>> GetAllByTimeAsync();
        Task<string?> DeductFIFO(Guid productId, int quantity);
        Task<bool> ReturnStock(string mapping);
    }
}
