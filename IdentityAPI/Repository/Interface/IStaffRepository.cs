using IdentityAPI.Models;

namespace IdentityAPI.Repository.Interface
{
    public interface IStaffRepository
    {
        Task<Staff?> GetByEmailAsync(string email);
    }
}
