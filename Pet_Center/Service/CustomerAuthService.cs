using Microsoft.EntityFrameworkCore;
using Pet_Center.Models;
using Pet_Center.Repository.Interface;
using Pet_Center.Security;
using Pet_Center.Service.Interface;

namespace Pet_Center.Service
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

        public async Task<string?> LoginAsync(string email, string password)
        {
            var customer = await _customerRepository.GetByEmailAsync(email);

            if (customer == null)
                return null;

            if (!_passwordService.Verify(password, customer.PasswordHash))
                return null;

            var roles = new List<string> { "ROLE_CUSTOMER" };

            return _jwtService.GenerateToken(customer.Email, roles);
        }
    }
}
