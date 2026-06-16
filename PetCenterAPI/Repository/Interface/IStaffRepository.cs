using PetCenterAPI.Models;

namespace PetCenterAPI.Repository.Interface
{
    /// <summary>Data access for staff management (CRUD + role/vet helpers).</summary>
    public interface IStaffRepository
    {
        Task<List<Staff>> GetAllAsync();
        Task<Staff?> GetByIdAsync(Guid staffId);

        Task<bool> EmailExistsAsync(string email, Guid? excludeStaffId = null);
        Task<bool> PhoneExistsAsync(string phoneNumber, Guid? excludeStaffId = null);
        Task<bool> LicenseNumberExistsAsync(string licenseNumber, Guid? excludeStaffId = null);

        Task<List<Role>> GetAssignableRolesAsync();
        Task<Role?> GetRoleByIdAsync(Guid roleId);

        /// <summary>Average star rating from active vet feedbacks for a staff member.</summary>
        Task<decimal> GetVetAverageRatingAsync(Guid staffId);

        Task<Staff> CreateAsync(Staff staff);
        Task UpdateAsync(Staff staff);
        Task SaveChangesAsync();
    }
}
