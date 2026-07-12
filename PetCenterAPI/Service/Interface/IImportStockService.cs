using PetCenterAPI.DTOs.Responses.Import;
using PetCenterAPI.DTOs.Requests.Import;

namespace PetCenterAPI.Service.Interface
{
    public interface IImportStockService
    {
        Task<Guid> CreateAsync(CreateImportRequestDTO dto, Guid staffGuid);
        Task<ReadImportResponseDTO?> GetByIdAsync(Guid id);
        Task ConfirmAsync(Guid id, Guid staffId);
        Task CancelAsync(Guid id);

        Task<List<ReadImportHeaderResponseDTO>> GetAllImportsAsync();
        Task<ExportResponseDTO> Export(DateTime? fromDate, DateTime? toDate);
        
        Task<bool> HasProductInImportsAsync(Guid productId);
    }
}
