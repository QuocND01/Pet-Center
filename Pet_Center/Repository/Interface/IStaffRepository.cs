using Pet_Center.Models;

namespace Pet_Center.Repository.Interface
{
    public interface IStaffRepository
    {
        Task<Staff?> GetByEmailAsync(string email);
    }
}
