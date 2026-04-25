using CustomerAPI.Models;
using CustomerAPI.Repository.Interface;
using CustomerAPI.Security;
using CustomerAPI.Service.Interface;
using System.Security.Cryptography;

namespace CustomerAPI.Service
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

            if (customer == null)
                return (false, "This email is not registered in our system.");

            if (customer.IsVerified != true)
                return (false, "This email has not been verified yet. Please complete your registration first.");

            if (customer.IsActive != true)
                return (false, "This account has been deactivated. Please contact support.");

            var token = GenerateSecureToken();
            var tokenHash = _passwordService.Hash(token);

            // Lưu reset token vào bảng OtpCode thay vì Customer
            var otp = await _customerRepository.GetOtpByCustomerIdAsync(customer.CustomerId);
            if (otp == null)
            {
                otp = new OtpCode
                {
                    OtpId = Guid.NewGuid(),
                    CustomerId = customer.CustomerId
                };
                otp.PasswordResetToken = tokenHash;
                otp.PasswordResetExpire = DateTime.UtcNow.AddMinutes(15);
                await _customerRepository.AddOtpAsync(otp);
            }
            else
            {
                otp.PasswordResetToken = tokenHash;
                otp.PasswordResetExpire = DateTime.UtcNow.AddMinutes(15);
                await _customerRepository.UpdateOtpAsync(otp);
            }

            // Không cần UpdateAsync(customer) nữa vì không còn lưu token trong Customer
            var clientBaseUrl = _configuration["ClientBaseUrl"] ?? "https://localhost:7010";
            var encodedToken = Uri.EscapeDataString(token);
            var encodedEmail = Uri.EscapeDataString(email);
            var resetLink = $"{clientBaseUrl}/Auth/ResetPassword?email={encodedEmail}&token={encodedToken}";

            await _emailService.SendResetPasswordEmailAsync(
                email, customer.FullName ?? "Customer", resetLink);

            return (true, "If this email is registered, you will receive a password reset link shortly.");
        }

        public async Task<(bool Valid, string Message)> ValidateResetTokenAsync(string email, string token)
        {
            var customer = await _customerRepository.GetByEmailAsyncWithoutActiveCheck(email);
            if (customer == null)
                return (false, "Invalid or expired reset link.");

            //Lấy token từ OtpCode
            var otp = await _customerRepository.GetOtpByCustomerIdAsync(customer.CustomerId);
            if (otp == null || string.IsNullOrEmpty(otp.PasswordResetToken) || otp.PasswordResetExpire == null)
                return (false, "Invalid or expired reset link.");

            if (otp.PasswordResetExpire < DateTime.UtcNow)
                return (false, "This reset link has expired. Please request a new one.");

            if (!_passwordService.Verify(token, otp.PasswordResetToken))
                return (false, "Invalid or expired reset link.");

            return (true, "Token is valid.");
        }

        public async Task<(bool Success, string Message)> ResetPasswordAsync(
            string email, string token, string newPassword)
        {
            var (valid, msg) = await ValidateResetTokenAsync(email, token);
            if (!valid) return (false, msg);

            var customer = await _customerRepository.GetByEmailAsyncWithoutActiveCheck(email);
            if (customer == null) return (false, "Invalid or expired reset link.");

            // Cập nhật password trong Customer
            customer.PasswordHash = _passwordService.Hash(newPassword);
            customer.UpdatedAt = DateTime.UtcNow;
            await _customerRepository.UpdateAsync(customer);

            var otp = await _customerRepository.GetOtpByCustomerIdAsync(customer.CustomerId);
            if (otp != null)
            {
                otp.PasswordResetToken = null;
                otp.PasswordResetExpire = null;
                await _customerRepository.UpdateOtpAsync(otp);
            }

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
