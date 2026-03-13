using IdentityAPI.Repository.Interface;
using IdentityAPI.Security;
using IdentityAPI.Service.Interface;
using Microsoft.EntityFrameworkCore;
using IdentityAPI.Models;
using IdentityAPI.DTOs.Resquest;
using IdentityAPI.DTOs.Response;

namespace IdentityAPI.Service
{
    public class CustomerAuthService : ICustomerAuthService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly PasswordService _passwordService;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;

        public CustomerAuthService(
            ICustomerRepository customerRepository,
            PasswordService passwordService,
            IJwtService jwtService,
            IEmailService emailService)
        {
            _customerRepository = customerRepository;
            _passwordService = passwordService;
            _jwtService = jwtService;
            _emailService = emailService;
        }

        // ============================================================
        // LOGIN
        // ============================================================
        public async Task<(bool success, string token, string errorType, string message)> LoginAsync(string email, string password)
        {
            var customer = await _customerRepository.GetByEmailAsyncWithoutActiveCheck(email);

            if (customer == null)
                return (false, null, "InvalidCredentials", "Email or password incorrect");

            if (!_passwordService.Verify(password, customer.PasswordHash))
                return (false, null, "InvalidCredentials", "Email or password incorrect");

            if (customer.EmailVerified != true)
                return (false, null, "EmailNotVerified", "Your account is not currently registered and verified. Please register again.");

            if (customer.IsActive != true)
                return (false, null, "AccountInactive", "Your account has been deactivated. Please contact support.");

            var roles = new List<string> { "Customer" };
            var token = _jwtService.GenerateToken(customer.CustomerId, customer.Email, roles);
            return (true, token, null, "Login success");
        }

        // ============================================================
        // REGISTER: Tạo Customer tạm (chưa active) + Gửi OTP
        // ============================================================
        public async Task<(bool Success, string Message)> RegisterAsync(RegisterDto dto)
        {
            var existing = await _customerRepository.GetByEmailAsyncWithoutActiveCheck(dto.Email);

            if (existing != null)
            {
                if (existing.EmailVerified == true)
                    return (false, "Email is already registered. Please login.");

                // Chưa verify → xóa record cũ, cho đăng ký lại
                await _customerRepository.DeleteAsync(existing);
            }

            var existingPhone = await _customerRepository.GetByPhoneAsync(dto.PhoneNumber);
            if (existingPhone != null)
                return (false, "Phone number is already in use by another account.");

            var code = GenerateOtp();

            var customer = new Customer
            {
                CustomerId = Guid.NewGuid(),
                Email = dto.Email,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                PasswordHash = _passwordService.Hash(dto.Password),
                BirthDay = dto.BirthDay,
                Gender = dto.Gender ?? "",
                EmailVerified = false,
                IsActive = false,
                VerificationCode = code,
                VerificationExpire = DateTime.UtcNow.AddMinutes(5),
                LastOtpSentAt = DateTime.UtcNow,
                OtpAttemptCount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _customerRepository.AddAsync(customer);
            await _emailService.SendVerificationEmail(dto.Email, code);

            return (true, "Verification code sent to your email. Please verify within 5 minutes.");
        }

        // ============================================================
        // VERIFY OTP → Kích hoạt tài khoản
        // ============================================================
        public async Task<(bool Success, string Message)> VerifyOtpAsync(VerifyOtpDto dto)
        {
            var customer = await _customerRepository.GetByEmailAsyncWithoutActiveCheck(dto.Email);

            if (customer == null)
                return (false, "Registration session not found. Please register again.");

            if (customer.EmailVerified == true)
                return (false, "Email already verified. Please login.");

            if (customer.OtpAttemptCount >= 5)
                return (false, "Too many incorrect attempts. Please register again.");

            if (customer.VerificationExpire < DateTime.UtcNow)
                return (false, "Verification code expired. Please resend OTP.");

            if (customer.VerificationCode != dto.Code)
            {
                customer.OtpAttemptCount = (customer.OtpAttemptCount ?? 0) + 1;
                customer.UpdatedAt = DateTime.UtcNow;
                await _customerRepository.UpdateAsync(customer);
                return (false, $"Invalid code. {5 - customer.OtpAttemptCount} attempts left.");
            }

            // OTP đúng → Kích hoạt tài khoản
            customer.EmailVerified = true;
            customer.IsActive = true;
            customer.VerificationCode = null;
            customer.VerificationExpire = null;
            customer.OtpAttemptCount = 0;
            customer.UpdatedAt = DateTime.UtcNow;

            await _customerRepository.UpdateAsync(customer);
            return (true, "Email verified successfully. You can now login.");
        }

        // ============================================================
        // RESEND OTP
        // ============================================================
        public async Task<(bool Success, string Message)> ResendOtpAsync(string email)
        {
            var customer = await _customerRepository.GetByEmailAsyncWithoutActiveCheck(email);

            if (customer == null)
                return (false, "Registration session not found. Please register again.");

            if (customer.EmailVerified == true)
                return (false, "Email already verified. Please login.");

            if (customer.LastOtpSentAt.HasValue &&
                (DateTime.UtcNow - customer.LastOtpSentAt.Value).TotalSeconds < 30)
            {
                var wait = 30 - (int)(DateTime.UtcNow - customer.LastOtpSentAt.Value).TotalSeconds;
                return (false, $"Please wait {wait} seconds before resending.");
            }

            var newCode = GenerateOtp();
            customer.VerificationCode = newCode;
            customer.VerificationExpire = DateTime.UtcNow.AddMinutes(5);
            customer.LastOtpSentAt = DateTime.UtcNow;
            customer.OtpAttemptCount = 0;
            customer.UpdatedAt = DateTime.UtcNow;

            await _customerRepository.UpdateAsync(customer);
            await _emailService.SendVerificationEmail(email, newCode);

            return (true, "New verification code sent.");
        }

        private static string GenerateOtp() =>
            new Random().Next(100000, 999999).ToString();

        public async Task<(bool Success, string Message)> ChangePasswordAsync(Guid customerId, ChangePasswordDto dto)
        {
            var customer = await _customerRepository.GetByEmailAsyncWithoutActiveCheck(
                (await _customerRepository.GetByIdAsync(customerId))?.Email ?? ""
            );

            if (customer == null)
                return (false, "Customer not found.");

            if (!_passwordService.Verify(dto.CurrentPassword, customer.PasswordHash))
                return (false, "Current password is incorrect.");

            customer.PasswordHash = _passwordService.Hash(dto.NewPassword);
            customer.UpdatedAt = DateTime.UtcNow;

            await _customerRepository.UpdateAsync(customer);
            return (true, "Password changed successfully.");
        }
    }

}

