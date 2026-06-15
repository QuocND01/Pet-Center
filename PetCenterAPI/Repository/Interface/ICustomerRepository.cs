using PetCenterAPI.Models;

namespace PetCenterAPI.Repository.Interface
{
    public interface ICustomerRepository
    {
        // ============================================================
        // LOGIN
        // ============================================================

        /// <summary>
        /// Get customer by email (only active accounts)
        /// </summary>
        Task<Customer?> GetByEmailAsync(string email);

        /// <summary>
        /// Get customer by email (include inactive accounts - used for login validation)
        /// </summary>
        Task<Customer?> GetByEmailAsyncWithoutActiveCheck(string email);
    }
}
