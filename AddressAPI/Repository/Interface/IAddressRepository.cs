using AddressAPI.Models;

namespace AddressAPI.Repository.Interface
{
    public interface IAddressRepository
    {
        Task<IEnumerable<Address>> GetAllAsync();
        Task<Address?> GetByIdAsync(Guid id);
        Task AddAsync(Address address);
        void Update(Address address);
        void Delete(Address address);
        Task<bool> SaveChangesAsync();
    }
}
