using PetCenterAPI.Models;

namespace PetCenterAPI.Repository.Interface
{
    public interface ISupplierRepository
    {
        Task<IEnumerable<Supplier>> GetAllAsync();
        Task<Supplier?> GetByIdAsync(Guid id);
        Task<Supplier?> FindDuplicateAsync(
    string taxId,
    string supplierName,
    string email,
    string phoneNumber,
    Guid? excludeSupplierId = null);
        Task AddAsync(Supplier supplier);
        void Update(Supplier supplier);
        Task SaveChangesAsync();
    }
}
