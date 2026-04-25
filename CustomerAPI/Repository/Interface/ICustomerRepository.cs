using CustomerAPI.Models;

namespace CustomerAPI.Repository.Interface
{
    public interface ICustomerRepository
    {
        // Login
        Task<Customer?> GetByEmailAsync(string email);
        Task<Customer?> GetByEmailAsyncWithoutActiveCheck(string email);

        // Register
        Task<bool> AddAsync(Customer customer);
        Task<bool> DeleteAsync(Customer customer);
        Task<Customer?> GetByPhoneAsync(string phone);

        // For Staff/Admin
        Task<List<Customer>> GetAllCustomersAsync();
        Task<Customer?> GetCustomerByIdAsync(Guid customerId);

        // For Customer
        Task<Customer?> GetByIdAsync(Guid customerId);
        Task<bool> UpdateAsync(Customer customer);

        // OTP
        Task<OtpCode?> GetOtpByCustomerIdAsync(Guid customerId);
        Task<bool> AddOtpAsync(OtpCode otp);
        Task<bool> UpdateOtpAsync(OtpCode otp);
        Task<bool> DeleteOtpAsync(OtpCode otp);
    }
}
