using IdentityAPI.Models;
using System.Threading.Tasks;

namespace IdentityAPI.Repository.Interface
{
    public interface ICustomerRepository
    {
        // ==================================== Login ====================================
        Task<Customer?> GetByEmailAsync(string email);

        Task<Customer?> GetByEmailAsyncWithoutActiveCheck(string email);


        // ==================================== For Staff and Admin ====================================
        Task<List<Customer>> GetAllCustomersAsync();

        Task<Customer?> GetCustomerByIdAsync(Guid customerId);

        // ==================================== For Customer ====================================

        Task<Customer?> GetByIdAsync(Guid customerId);

        Task<bool> UpdateAsync(Customer customer);
    }
}
