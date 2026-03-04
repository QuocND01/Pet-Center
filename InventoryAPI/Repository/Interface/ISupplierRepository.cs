using InventoryAPI.Models;

namespace InventoryAPI.Repository.Interface
{
    public interface ISupplierRepository
    {
        Task<IEnumerable<Supplier>> GetAllAsync();
        Task<Supplier?> GetByIdAsync(Guid id);
        Task AddAsync(Supplier supplier);
        void Update(Supplier supplier);
        Task SaveChangesAsync();
    }
}
