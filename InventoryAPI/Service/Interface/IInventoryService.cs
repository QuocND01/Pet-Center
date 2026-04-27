using InventoryAPI.DTOs;

namespace InventoryAPI.Service.Interface
{
    public interface IInventoryService
    {
        Task<IEnumerable<ProductQuantityDTO>> GetProductStockAsync(List<Guid> productIds);

        IQueryable<ReadInventoryDto> GetInventories();
        Task<ReadInventoryDto?> GetInventoryById(Guid id);
    }
}
