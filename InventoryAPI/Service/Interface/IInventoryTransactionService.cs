using InventoryAPI.DTOs;

namespace InventoryAPI.Service.Interface
   
{
    public interface IInventoryTransactionService
    {
        IQueryable<ReadTransactionDto> GetTransactions();
        Task<List<ReadTransactionDto>> GetByInventoryId(Guid inventoryId);
    }
}
