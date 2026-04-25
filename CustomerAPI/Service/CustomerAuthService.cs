using CustomerAPI.DTOs.Request;
using CustomerAPI.DTOs.Response;
using CustomerAPI.Models;
using CustomerAPI.Repository.Interface;
using CustomerAPI.Security;
using CustomerAPI.Service.Interface;

namespace CustomerAPI.Service
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
        public async Task<(bool success, string? token, string? errorType, string message)> LoginAsync(
            string email, string password)
        {
            var customer = await _customerRepository.GetByEmailAsyncWithoutActiveCheck(email);

            if (customer == null)
                return (false, null, "InvalidCredentials", "Email or password incorrect");

            if (customer.PasswordHash == null || !_passwordService.Verify(password, customer.PasswordHash))
                return (false, null, "InvalidCredentials", "Email or password incorrect");

            // IsVerified thay EmailVerified
            if (customer.IsVerified != true)
                return (false, null, "EmailNotVerified",
                    "Your account is not verified. Please register again.");

            if (customer.IsActive != true)
                return (false, null, "AccountInactive",
                    "Your account has been deactivated. Please contact support.");

            var token = _jwtService.GenerateToken(
                customer.CustomerId, customer.Email!, new List<string> { "Customer" });

            return (true, token, null, "Login success");
        }

        // ============================================================
        // REGISTER — Tạo Customer tạm + tạo OtpCode riêng + gửi email
        // ============================================================
        public async Task<(bool Success, string Message)> RegisterAsync(RegisterDto dto)
        {
            var existing = await _customerRepository.GetByEmailAsyncWithoutActiveCheck(dto.Email);

            if (existing != null)
            {
                // IsVerified thay EmailVerified
                if (existing.IsVerified == true)
                    return (false, "Email is already registered. Please login.");

                // Chưa verify → xóa OTP cũ rồi xóa customer, cho đăng ký lại
                var existingOtp = await _customerRepository.GetOtpByCustomerIdAsync(existing.CustomerId);
                if (existingOtp != null)
                    await _customerRepository.DeleteOtpAsync(existingOtp);

                await _customerRepository.DeleteAsync(existing);
            }

            var existingPhone = await _customerRepository.GetByPhoneAsync(dto.PhoneNumber);
            if (existingPhone != null)
                return (false, "Phone number is already in use by another account.");

            // Tạo customer mới (chưa verify, chưa active)
            var customer = new Customer
            {
                CustomerId = Guid.NewGuid(),
                Email = dto.Email,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                PasswordHash = _passwordService.Hash(dto.Password),
                BirthDay = dto.BirthDay,
                Gender = dto.Gender ?? "",
                IsVerified = false,
                IsActive = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _customerRepository.AddAsync(customer);

            // Tạo OtpCode riêng trong bảng OtpCodes
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

            if (customer.IsVerified == true) 
                return (false, "Email already verified. Please login.");

            // Lấy OTP từ bảng riêng
            var otp = await _customerRepository.GetOtpByCustomerIdAsync(customer.CustomerId);
            if (otp == null)
                return (false, "No OTP found. Please register again.");

            if (otp.OtpAttemptCount >= 5)
                return (false, "Too many incorrect attempts. Please register again.");

            if (otp.VerificationExpire < DateTime.UtcNow)
                return (false, "Verification code expired. Please resend OTP.");

            if (otp.VerificationCode != dto.Code)
            {
                otp.OtpAttemptCount = (otp.OtpAttemptCount ?? 0) + 1;
                await _customerRepository.UpdateOtpAsync(otp);
                return (false, $"Invalid code. {5 - otp.OtpAttemptCount} attempts left.");
            }

            // OTP đúng → kích hoạt customer
            customer.IsVerified = true;
            customer.IsActive = true;
            customer.UpdatedAt = DateTime.UtcNow;
            await _customerRepository.UpdateAsync(customer);

            // Xóa OTP sau khi verify xong
            await _customerRepository.DeleteOtpAsync(otp);

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

            if (customer.IsVerified == true) 
                return (false, "Email already verified. Please login.");

            var otp = await _customerRepository.GetOtpByCustomerIdAsync(customer.CustomerId);

            // Kiểm tra cooldown 30 giây
            if (otp?.LastOtpSentAt.HasValue == true &&
                (DateTime.UtcNow - otp.LastOtpSentAt.Value).TotalSeconds < 30)
            {
                var wait = 30 - (int)(DateTime.UtcNow - otp.LastOtpSentAt.Value).TotalSeconds;
                return (false, $"Please wait {wait} seconds before resending.");
            }

            var newCode = GenerateOtp();

            if (otp == null)
            {
                // Tạo mới nếu chưa có (edge case)
                otp = new OtpCode
                {
                    OtpId = Guid.NewGuid(),
                    CustomerId = customer.CustomerId
                };
                otp.VerificationCode = newCode;
                otp.VerificationExpire = DateTime.UtcNow.AddMinutes(5);
                otp.LastOtpSentAt = DateTime.UtcNow;
                otp.OtpAttemptCount = 0;
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
            Guid customerId, ChangePasswordResponseDto dto)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
                return (false, "Customer not found.");

            if (customer.PasswordHash == null ||
                !_passwordService.Verify(dto.CurrentPassword, customer.PasswordHash))
                return (false, "Current password is incorrect.");

            customer.PasswordHash = _passwordService.Hash(dto.NewPassword);
            customer.UpdatedAt = DateTime.UtcNow;

            await _customerRepository.UpdateAsync(customer);
            return (true, "Password changed successfully.");
        }

        private static string GenerateOtp() =>
            new Random().Next(100000, 999999).ToString();
    }
}
