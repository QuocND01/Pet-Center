using IdentityAPI.Repository.Interface;
using IdentityAPI.Security;
using IdentityAPI.Service.Interface;
using Microsoft.EntityFrameworkCore;
using IdentityAPI.Models;

namespace IdentityAPI.Service
{
    public class CustomerAuthService : ICustomerAuthService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly PasswordService _passwordService;
        private readonly IJwtService _jwtService;

        public CustomerAuthService(
            ICustomerRepository customerRepository,
            PasswordService passwordService,
            IJwtService jwtService)
        {
            _customerRepository = customerRepository;
            _passwordService = passwordService;
            _jwtService = jwtService;
        }

        public async Task<(bool success, string token, string errorType, string message)> LoginAsync(string email, string password)
        {
            // Tìm customer bằng email (không check IsActive)
            var customer = await _customerRepository.GetByEmailAsyncWithoutActiveCheck(email);
            if (customer == null)
            {
                return (false, null, "InvalidCredentials", "Email or password incorrect");
            }

            // Kiểm tra password
            if (!_passwordService.Verify(password, customer.PasswordHash))
            {
                return (false, null, "InvalidCredentials", "Email or password incorrect");
            }

            // ✅ Kiểm tra account có active không (sử dụng != true để handle nullable)
            if (customer.IsActive != true)
            {
                return (false, null, "AccountInactive", "Your account has been deactivated. Please contact support.");
            }

            // Login thành công
            var roles = new List<string> { "Customer" };
            var token = _jwtService.GenerateToken(customer.CustomerId, customer.Email, roles);

            return (true, token, null, "Login success");
        }
    }
}
