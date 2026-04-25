using StaffAPI.Models;

namespace StaffAPI.Repositories.Interfaces
{
    public interface IStaffAuthRepository
    {
        Task<Staff?> GetByEmailAsync(string email);
        Task<Staff?> GetByIdAsync(Guid staffId);
        Task<List<Staff>> GetAllAsync();
        Task<bool> UpdateAsync(Staff staff);
    }
}
