using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Security;
using PetCenterAPI.Service.Interface;

namespace PetCenterAPI.Service
{
    public class CustomerAuthService : ICustomerAuthService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly PasswordService _passwordService;
        private readonly IJwtService _jwtService;
        // private readonly IEmailService _emailService;

        public CustomerAuthService(
            ICustomerRepository customerRepository,
            PasswordService passwordService,
            IJwtService jwtService
            //IEmailService emailService
            )
        {
            _customerRepository = customerRepository;
            _passwordService = passwordService;
            _jwtService = jwtService;
            //_emailService = emailService;
        }

        // ============================================================
        // LOGIN
        // ============================================================

        /// <summary>
        /// Validate email and password, check verification and active status,
        /// then return a signed JWT token on success
        /// </summary>
        public async Task<(bool success, string? token, string? errorType, string message)> LoginAsync(
            string email, string password)
        {
            var customer = await _customerRepository.GetByEmailAsyncWithoutActiveCheck(email);

            if (customer == null)
                return (false, null, "InvalidCredentials", "Email or password incorrect");

            if (customer.PasswordHash == null || !_passwordService.Verify(password, customer.PasswordHash))
                return (false, null, "InvalidCredentials", "Email or password incorrect");

            if (customer.IsVerified != true)
                return (false, null, "EmailNotVerified",
                    "Your account is not verified. Please register again.");

            if (customer.IsActive != true)
                return (false, null, "AccountInactive",
                    "Your account has been deactivated. Please contact support.");

            var token = _jwtService.GenerateToken(
                customer.CustomerId,
                customer.Email!,
                new List<string> { "Customer" },
                customer.FullName ?? "");

            return (true, token, null, "Login success");
        }
    }
}
