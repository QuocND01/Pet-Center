using InventoryAPI.DTOs;

namespace InventoryAPI.Service.Interface
{
    public interface IInventoryService
    {
        Task<IEnumerable<ProductInventoryDTO>> GetProductStockAsync(List<Guid> productIds);
    }
}
