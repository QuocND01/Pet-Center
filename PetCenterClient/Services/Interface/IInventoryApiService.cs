using PetCenterClient.ViewModels.Inventory;

namespace PetCenterClient.Services.Interface
{
    public interface IInventoryApiService
    {
        Task<InventoryListResponseViewModel?> GetPagedAsync();
        Task<InventoryDetailViewModel?> GetByIdAsync(Guid id);
    }
}
