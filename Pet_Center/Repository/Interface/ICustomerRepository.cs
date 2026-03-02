using ProductAPI.Models;
using System.Threading.Tasks;

namespace ProductAPI.Repository.Interface
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByEmailAsync(string email);
    }
}
