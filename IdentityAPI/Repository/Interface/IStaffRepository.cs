using IdentityAPI.Models;

namespace IdentityAPI.Repository.Interface
{
    public interface IStaffRepository
    {
        Task<Staff?> GetByEmailAsync(string email);
        Task<IEnumerable<Staff>> GetAllAsync();
        Task<Staff?> GetByIdAsync(Guid id);
        Task<IEnumerable<Staff>> SearchAsync(string keyword);
        Task AddAsync(Staff staff);
        Task UpdateAsync(Staff staff);
        Task DeleteAsync(Guid id);
        Task<bool> EmailExistsAsync(string email);
    }
}
