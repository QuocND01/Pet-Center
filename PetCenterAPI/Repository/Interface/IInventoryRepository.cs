using PetCenterAPI.Models;

namespace PetCenterAPI.Repository.Interface
{
    public interface IInventoryRepository
    {
        // Hàm này sẽ đi tìm record kho dựa vào ProductId
        Task<Inventory?> GetInventoryByProductIdAsync(Guid productId);
    }
}