using ProductAPI.Models;

namespace ProductAPI.Repository.Interface
{
    public interface IStaffRepository
    {
        Task<Staff?> GetByEmailAsync(string email);
    }
}
