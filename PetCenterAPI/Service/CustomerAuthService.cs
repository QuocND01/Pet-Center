using Microsoft.AspNetCore.Identity.Data;
using PetCenterAPI.DTOs.Requests.CustomerProfile;
using PetCenterAPI.DTOs.Requests.Register;
using PetCenterAPI.Models;
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
        private readonly IEmailService _emailService;

        public CustomerAuthService(
            ICustomerRepository customerRepository,
            PasswordService passwordService,
            IJwtService jwtService,
            IEmailService emailService
            )
        {
            _customerRepository = customerRepository;
            _passwordService = passwordService;
            _jwtService = jwtService;
            _emailService = emailService;
        }

        // ============================================================
        // LOGIN
        // ============================================================
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

        // ============================================================
        // REGISTER
        // ============================================================
        public async Task<(bool Success, string Message)> RegisterAsync(RegisterRequestDTO request)
        {
            var existing = await _customerRepository.GetByEmailAsyncWithoutActiveCheck(request.Email);

            if (existing != null)
            {
                if (existing.IsVerified == true)
                    return (false, "Email is already registered. Please login.");

                var existingOtp = await _customerRepository.GetOtpByCustomerIdAsync(existing.CustomerId);
                if (existingOtp != null)
                    await _customerRepository.DeleteOtpAsync(existingOtp);

                await _customerRepository.DeleteAsync(existing);
            }

            var existingPhone = await _customerRepository.GetByPhoneAsync(request.PhoneNumber);
            if (existingPhone != null)
                return (false, "Phone number is already in use by another account.");

            var customer = new Customer
            {
                CustomerId = Guid.NewGuid(),
                Email = request.Email,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = _passwordService.Hash(request.Password),
                BirthDay = request.BirthDay,
                Gender = request.Gender ?? "",
                IsVerified = false,
                IsActive = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _customerRepository.AddAsync(customer);

            var code = GenerateOtp();
            var otp = new OtpCode
            {
                OtpId = Guid.NewGuid(),
                CustomerId = customer.CustomerId,
                VerificationCode = code,
                VerificationExpire = DateTime.UtcNow.AddMinutes(5),
                LastOtpSentAt = DateTime.UtcNow,
                OtpAttemptCount = 0
            };
            await _customerRepository.AddOtpAsync(otp);

            await _emailService.SendVerificationEmail(request.Email, code);

            return (true, "Verification code sent to your email. Please verify within 5 minutes.");
        }

        // ============================================================
        // OTP — VERIFY
        // ============================================================
        public async Task<(bool Success, string Message)> VerifyOtpAsync(VerifyOtpRequestDTO request)
        {
            var customer = await _customerRepository.GetByEmailAsyncWithoutActiveCheck(request.Email);
            if (customer == null)
                return (false, "Registration session not found. Please register again.");

            if (customer.IsVerified == true)
                return (false, "Email already verified. Please login.");

            var otp = await _customerRepository.GetOtpByCustomerIdAsync(customer.CustomerId);
            if (otp == null)
                return (false, "No OTP found. Please register again.");

            if (otp.OtpAttemptCount >= 5)
                return (false, "Too many incorrect attempts. Please register again.");

            if (otp.VerificationExpire < DateTime.UtcNow)
                return (false, "Verification code expired. Please resend OTP.");

            if (otp.VerificationCode != request.Code)
            {
                otp.OtpAttemptCount = (otp.OtpAttemptCount ?? 0) + 1;
                await _customerRepository.UpdateOtpAsync(otp);
                return (false, $"Invalid code. {5 - otp.OtpAttemptCount} attempts left.");
            }

            customer.IsVerified = true;
            customer.IsActive = true;
            customer.UpdatedAt = DateTime.UtcNow;
            await _customerRepository.UpdateAsync(customer);

            await _customerRepository.DeleteOtpAsync(otp);

            return (true, "Email verified successfully. You can now login.");
        }

        // ============================================================
        // OTP — RESEND
        // ============================================================
        public async Task<(bool Success, string Message)> ResendOtpAsync(string email)
        {
            var customer = await _customerRepository.GetByEmailAsyncWithoutActiveCheck(email);
            if (customer == null)
                return (false, "Registration session not found. Please register again.");

            if (customer.IsVerified == true)
                return (false, "Email already verified. Please login.");

            var otp = await _customerRepository.GetOtpByCustomerIdAsync(customer.CustomerId);

            if (otp?.LastOtpSentAt.HasValue == true &&
                (DateTime.UtcNow - otp.LastOtpSentAt.Value).TotalSeconds < 30)
            {
                var wait = 30 - (int)(DateTime.UtcNow - otp.LastOtpSentAt.Value).TotalSeconds;
                return (false, $"Please wait {wait} seconds before resending.");
            }

            var newCode = GenerateOtp();

            if (otp == null)
            {
                otp = new OtpCode
                {
                    OtpId = Guid.NewGuid(),
                    CustomerId = customer.CustomerId,
                    VerificationCode = newCode,
                    VerificationExpire = DateTime.UtcNow.AddMinutes(5),
                    LastOtpSentAt = DateTime.UtcNow,
                    OtpAttemptCount = 0
                };
                await _customerRepository.AddOtpAsync(otp);
            }
            else
            {
                otp.VerificationCode = newCode;
                otp.VerificationExpire = DateTime.UtcNow.AddMinutes(5);
                otp.LastOtpSentAt = DateTime.UtcNow;
                otp.OtpAttemptCount = 0;
                await _customerRepository.UpdateOtpAsync(otp);
            }

            await _emailService.SendVerificationEmail(email, newCode);
            return (true, "New verification code sent.");
        }

        // ============================================================
        // CHANGE PASSWORD
        // ============================================================
        public async Task<(bool Success, string Message)> ChangePasswordAsync(
            Guid customerId, ChangePasswordRequestDTO request)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
                return (false, "Customer not found.");

            if (customer.PasswordHash == null ||
                !_passwordService.Verify(request.CurrentPassword, customer.PasswordHash))
                return (false, "Current password is incorrect.");

            customer.PasswordHash = _passwordService.Hash(request.NewPassword);
            customer.UpdatedAt = DateTime.UtcNow;
            await _customerRepository.UpdateAsync(customer);

            return (true, "Password changed successfully.");
        }

        // ============================================================
        // HELPER
        // ============================================================
        private static string GenerateOtp()
            => new Random().Next(100000, 999999).ToString();
    }
}
