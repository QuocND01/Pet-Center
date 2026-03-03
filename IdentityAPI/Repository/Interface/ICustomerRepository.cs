using IdentityAPI.Models;
using System.Threading.Tasks;

namespace IdentityAPI.Repository.Interface
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByEmailAsync(string email);

        Task<List<Customer>> GetAllCustomersAsync();

        Task<Customer?> GetCustomerByIdAsync(Guid customerId);
    }
}
