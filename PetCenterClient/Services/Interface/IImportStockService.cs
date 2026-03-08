using PetCenterClient.DTOs;

namespace PetCenterClient.Services.Interface
{
    public interface IImportStockService
    {
        Task<List<ReadImportHeaderDto>> GetAllAsync();
        Task<ReadImportStockDto?> GetByIdAsync(Guid id);
        Task<Guid> CreateAsync(CreateImportStockDto dto);
        Task ConfirmAsync(Guid id);
        Task CancelAsync(Guid id);
    }
}
