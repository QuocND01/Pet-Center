using Pet_Center.Models;
using System.Threading.Tasks;

namespace Pet_Center.Repository.Interface
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByEmailAsync(string email);
    }
}
