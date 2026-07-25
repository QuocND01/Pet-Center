using PetCenterAPI.Models;

namespace PetCenterAPI.Repository.Interface
{
    public interface ISupplierRepository
    {
        Task<IEnumerable<Supplier>> GetAllAsync();
        Task<Supplier?> GetByIdAsync(Guid id);
        Task<bool> GetByTaxIdAsync(string taxId);
        Task AddAsync(Supplier supplier);
        void Update(Supplier supplier);
        Task SaveChangesAsync();
    }
}
