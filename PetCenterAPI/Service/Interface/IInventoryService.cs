using PetCenterAPI.DTOs.Requests.Inventory;
using PetCenterAPI.DTOs.Responses.Inventory;

namespace PetCenterAPI.Service.Interface
{
    public interface IInventoryService
    {
        Task<InventoryListResponseDTO> GetPagedAsync(
            InventoryQueryRequestDTO request);

        Task<InventoryDetailResponseDTO?> GetByIdAsync(Guid inventoryId);
    }
}
