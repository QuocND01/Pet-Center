using PetCenterAPI.DTOs.Requests.Inventory;
using PetCenterAPI.DTOs.Responses.Inventory;
using PetCenterAPI.Models;

namespace PetCenterAPI.Repository.Interface
{
    public interface IInventoryRepository
    {
        // Hàm này sẽ đi tìm record kho dựa vào ProductId
        Task<Inventory?> GetInventoryByProductIdAsync(Guid productId);

        //Inventory module-Vinh
        Task<(List<Inventory> Items, int TotalRecords)> GetPagedAsync(
            InventoryQueryRequestDTO request);

        Task<Inventory?> GetByIdAsync(Guid inventoryId);
        Task<List<ImportStockDetail>> GetAvailableBatchesByProductIdAsync(Guid productId);
    }
}