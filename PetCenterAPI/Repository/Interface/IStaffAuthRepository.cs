using PetCenterAPI.Models;

namespace PetCenterAPI.Repository.Interface
{
    public interface IStaffAuthRepository
    {
        // ============================================================
        // LOGIN
        // ============================================================

        /// <summary>
        /// Get staff by email including their roles, used for login validation
        /// </summary>
        Task<Staff?> GetByEmailAsync(string email);

        /// <summary>
        /// Get staff by ID including their roles
        /// </summary>
        Task<Staff?> GetByIdAsync(Guid staffId);

        /// <summary>
        /// Get all staff including their roles
        /// </summary>
        Task<List<Staff>> GetAllAsync();

        /// <summary>
        /// Update staff record and persist changes
        /// </summary>
        Task<bool> UpdateAsync(Staff staff);
    }
}
