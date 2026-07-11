using PetCenterAPI.Models;

namespace PetCenterAPI.Repository.Interface
{
    public interface IInventoryTransactionRepository
    {
        Task AddTransactionAsync(
        InventoryTransaction transaction);
        Task SaveChange();
    }
}
