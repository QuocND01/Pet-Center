using PetCenterAPI.DTOs.Requests.Inventory;
using PetCenterAPI.DTOs.Responses.Inventory;

namespace PetCenterAPI.Service.Interface
{
    public interface IInventoryService
    {
        Task<InventoryListResponseDTO> GetPagedAsync(
            InventoryQueryRequestDTO request);

        Task<InventoryDetailResponseDTO?> GetByIdAsync(Guid inventoryId);
        Task<IEnumerable<InventoryReportDto>> GetInventorySummaryAsync();
        Task<IEnumerable<BatchExpiryReportDto>> GetBatchExpiryReportAsync();
        Task ProcessExpiredBatchesAsync(CancellationToken cancellationToken = default);
        Task<byte[]> GenerateInventoryExcelReportAsync();
    }
}
