using IdentityAPI.Repository.Interface;
using IdentityAPI.Security;
using IdentityAPI.Service.Interface;
using System.Security.Cryptography;

namespace IdentityAPI.Service
{
    public class ForgotPasswordService : IForgotPasswordService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IEmailService _emailService;
        private readonly PasswordService _passwordService;
        private readonly IConfiguration _configuration;

        public ForgotPasswordService(
            ICustomerRepository customerRepository,
            IEmailService emailService,
            PasswordService passwordService,
            IConfiguration configuration)
        {
            _customerRepository = customerRepository;
            _emailService = emailService;
            _passwordService = passwordService;
            _configuration = configuration;
        }

        public async Task<(bool Success, string Message)> SendResetPasswordEmailAsync(string email)
        {
            var customer = await _customerRepository.GetByEmailAsyncWithoutActiveCheck(email);

            // Luôn trả về success để tránh email enumeration attack
            if (customer == null)
                return (false, "This email is not registered in our system.");

            if (customer.EmailVerified != true)
                return (false, "This email has not been verified yet. Please complete your registration first.");

            if (customer.IsActive != true)
                return (false, "This account has been deactivated. Please contact support.");

            var token = GenerateSecureToken();
            var tokenHash = _passwordService.Hash(token);

            customer.PasswordResetToken = tokenHash;
            customer.PasswordResetExpire = DateTime.UtcNow.AddMinutes(15);
            customer.UpdatedAt = DateTime.UtcNow;

            await _customerRepository.UpdateAsync(customer);

            var clientBaseUrl = _configuration["ClientBaseUrl"] ?? "https://localhost:7010";
            var encodedToken = Uri.EscapeDataString(token);
            var encodedEmail = Uri.EscapeDataString(email);
            var resetLink = $"{clientBaseUrl}/Auth/ResetPassword?email={encodedEmail}&token={encodedToken}";

            await _emailService.SendResetPasswordEmailAsync(email, customer.FullName ?? "Customer", resetLink);

            return (true, "If this email is registered, you will receive a password reset link shortly.");
        }

        public async Task<(bool Valid, string Message)> ValidateResetTokenAsync(string email, string token)
        {
            var customer = await _customerRepository.GetByEmailAsyncWithoutActiveCheck(email);

            if (customer == null)
                return (false, "Invalid or expired reset link.");

            if (string.IsNullOrEmpty(customer.PasswordResetToken) || customer.PasswordResetExpire == null)
                return (false, "Invalid or expired reset link.");

            if (customer.PasswordResetExpire < DateTime.UtcNow)
                return (false, "This reset link has expired. Please request a new one.");

            if (!_passwordService.Verify(token, customer.PasswordResetToken))
                return (false, "Invalid or expired reset link.");

            return (true, "Token is valid.");
        }

        public async Task<(bool Success, string Message)> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var (valid, msg) = await ValidateResetTokenAsync(email, token);
            if (!valid) return (false, msg);

            var customer = await _customerRepository.GetByEmailAsyncWithoutActiveCheck(email);
            if (customer == null) return (false, "Invalid or expired reset link.");

            customer.PasswordHash = _passwordService.Hash(newPassword);
            customer.PasswordResetToken = null;
            customer.PasswordResetExpire = null;
            customer.UpdatedAt = DateTime.UtcNow;

            await _customerRepository.UpdateAsync(customer);

            return (true, "Your password has been reset successfully. You can now login.");
        }

        private static string GenerateSecureToken()
        {
            var bytes = new byte[48];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }
    }
}
