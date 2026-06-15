using PetCenterAPI.Models;

namespace PetCenterAPI.Repository.Interface
{
    public interface ICustomerRepository
    {
        // ============================================================
        // LOGIN
        // ============================================================
        Task<Customer?> GetByEmailAsync(string email);
        Task<Customer?> GetByEmailAsyncWithoutActiveCheck(string email);

        // ============================================================
        // REGISTER
        // ============================================================
        Task<bool> AddAsync(Customer customer);
        Task<bool> DeleteAsync(Customer customer);
        Task<Customer?> GetByPhoneAsync(string phone);
        Task<bool> UpdateAsync(Customer customer);
        Task<Customer?> GetByIdInternalAsync(Guid customerId);

        // ============================================================
        // OTP
        // ============================================================
        Task<OtpCode?> GetOtpByCustomerIdAsync(Guid customerId);
        Task<bool> AddOtpAsync(OtpCode otp);
        Task<bool> UpdateOtpAsync(OtpCode otp);
        Task<bool> DeleteOtpAsync(OtpCode otp);

        // ============================================================
        // STAFF / ADMIN — CUSTOMER MANAGEMENT
        // ============================================================
        Task<List<Customer>> GetAllCustomersAsync();
        Task<Customer?> GetCustomerByIdAsync(Guid customerId);

        // ============================================================
        // CUSTOMER PROFILE
        // ============================================================
        Task<Customer?> GetByIdAsync(Guid customerId);
    }
}
