using AddressAPI.Models;

namespace AddressAPI.Repository.Interface
{
    public interface IAddressRepository
    {
        Task<IEnumerable<Address>> GetAllAsync();

        // Lấy thẳng từ DB theo customerId + IsActive = true
        Task<IEnumerable<Address>> GetByCustomerIdAsync(Guid customerId);

        Task<Address?> GetByIdAsync(Guid id);
        Task AddAsync(Address address);
        void Update(Address address);
        void Delete(Address address);
        Task<bool> SaveChangesAsync();
    }
}