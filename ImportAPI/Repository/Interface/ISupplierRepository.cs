using ImportAPI.Models;

namespace ImportAPI.Repository.Interface
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
