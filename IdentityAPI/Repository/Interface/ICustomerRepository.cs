using IdentityAPI.Models;
using System.Threading.Tasks;

namespace IdentityAPI.Repository.Interface
{
    public interface ICustomerRepository
    {
        // ==================================== Login ====================================
        Task<Customer?> GetByEmailAsync(string email);

        Task<Customer?> GetByEmailAsyncWithoutActiveCheck(string email);


        // ==================================== Register ====================================
        Task<bool> AddAsync(Customer customer);

        Task<bool> DeleteAsync(Customer customer);
        Task<Customer?> GetByPhoneAsync(string phone);


        // ==================================== For Staff and Admin ====================================
        Task<List<Customer>> GetAllCustomersAsync();

        Task<Customer?> GetCustomerByIdAsync(Guid customerId);

        // ==================================== For Customer ====================================

        Task<Customer?> GetByIdAsync(Guid customerId);

        Task<bool> UpdateAsync(Customer customer);
    }
}
