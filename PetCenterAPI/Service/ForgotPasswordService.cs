using System.Security.Cryptography;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Security;
using PetCenterAPI.Service.Interface;

namespace PetCenterAPI.Service
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

        // ============================================================
        // FORGOT PASSWORD — SEND RESET LINK
        // ============================================================
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

            var otp = await _customerRepository.GetOtpByCustomerIdAsync(customer.CustomerId);
            if (otp == null)
            {
                otp = new OtpCode
                {
                    OtpId = Guid.NewGuid(),
                    CustomerId = customer.CustomerId,
                    PasswordResetToken = tokenHash,
                    PasswordResetExpire = DateTime.UtcNow.AddMinutes(15)
                };
                await _customerRepository.AddOtpAsync(otp);
            }
            else
            {
                otp.PasswordResetToken = tokenHash;
                otp.PasswordResetExpire = DateTime.UtcNow.AddMinutes(15);
                await _customerRepository.UpdateOtpAsync(otp);
            }

            var clientBaseUrl = _configuration["ClientBaseUrl"] ?? "https://localhost:7010";
            var encodedToken = Uri.EscapeDataString(token);
            var encodedEmail = Uri.EscapeDataString(email);
            var resetLink = $"{clientBaseUrl}/Auth/ResetPassword?email={encodedEmail}&token={encodedToken}";

            await _emailService.SendResetPasswordEmailAsync(
                email, customer.FullName ?? "Customer", resetLink);

            return (true, "If this email is registered, you will receive a password reset link shortly.");
        }

        // ============================================================
        // FORGOT PASSWORD — VALIDATE TOKEN
        // ============================================================
        public async Task<(bool Valid, string Message)> ValidateResetTokenAsync(string email, string token)
        {
            var customer = await _customerRepository.GetByEmailAsyncWithoutActiveCheck(email);
            if (customer == null)
                return (false, "Invalid or expired reset link.");

            var otp = await _customerRepository.GetOtpByCustomerIdAsync(customer.CustomerId);
            if (otp == null || string.IsNullOrEmpty(otp.PasswordResetToken) || otp.PasswordResetExpire == null)
                return (false, "Invalid or expired reset link.");

            if (otp.PasswordResetExpire < DateTime.UtcNow)
                return (false, "This reset link has expired. Please request a new one.");

            if (!_passwordService.Verify(token, otp.PasswordResetToken))
                return (false, "Invalid or expired reset link.");

            return (true, "Token is valid.");
        }

        // ============================================================
        // FORGOT PASSWORD — RESET PASSWORD
        // ============================================================
        public async Task<(bool Success, string Message)> ResetPasswordAsync(
            string email, string token, string newPassword)
        {
            var (valid, msg) = await ValidateResetTokenAsync(email, token);
            if (!valid) return (false, msg);

            var customer = await _customerRepository.GetByEmailAsyncWithoutActiveCheck(email);
            if (customer == null) return (false, "Invalid or expired reset link.");

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

        // ============================================================
        // HELPER
        // ============================================================
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
