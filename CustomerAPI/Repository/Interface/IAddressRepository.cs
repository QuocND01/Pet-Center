using CustomerAPI.Models;

namespace CustomerAPI.Repository.Interface
{
    public interface IAddressRepository
    {
        Task<List<Address>> GetByCustomerIdAsync(Guid customerId);
        Task<Address?> GetByIdAsync(Guid id);
        Task AddAsync(Address address);
        void Update(Address address);
        void Delete(Address address);
        Task SaveChangesAsync();
    }
}
