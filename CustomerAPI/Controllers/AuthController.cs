using System.Security.Claims;
using CustomerAPI.DTOs.Request;
using CustomerAPI.DTOs.Response;
using CustomerAPI.Security;
using CustomerAPI.Service.Interface;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ICustomerAuthService _customerAuthService;
        private readonly IGoogleAuthService _googleAuthService;
        private readonly IJwtService _jwtService;
        private readonly IForgotPasswordService _forgotPasswordService;

        public AuthController(ICustomerAuthService customerAuthService, IGoogleAuthService googleAuthService, IJwtService jwtService, IForgotPasswordService forgotPasswordService)
        {
            _customerAuthService = customerAuthService;
            _googleAuthService = googleAuthService;
            _jwtService = jwtService;
            _forgotPasswordService = forgotPasswordService;
        }

        // POST: api/auth/customer-login
        [HttpPost("customer-login")]
        [AllowAnonymous]
        public async Task<IActionResult> CustomerLogin([FromBody] LoginRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, token, errorType, message) = await _customerAuthService.LoginAsync(dto.Email, dto.Password);

            if (!success)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = message,
                    errorType = errorType
                });
            }

            return Ok(new
            {
                success = true,
                message = "Login success",
                token = token
            });
        }


        // POST: api/auth/register
        // STEP 1: Nhận form → Tạo Customer tạm + Gửi OTP
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _customerAuthService.RegisterAsync(dto);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        // POST: api/auth/verify-otp
        // STEP 2: Verify OTP → Kích hoạt tài khoản
        [HttpPost("verify-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _customerAuthService.VerifyOtpAsync(dto);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        // POST: api/auth/resend-otp
        [HttpPost("resend-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _customerAuthService.ResendOtpAsync(dto.Email);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        // POST: api/auth/change-password
        [Authorize(Roles = "Customer")]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordResponseDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var customerIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(customerIdStr, out var customerId))
                return Unauthorized();

            var result = await _customerAuthService.ChangePasswordAsync(customerId, dto);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        // POST: api/auth/google-login
        [HttpPost("google-login")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequestDto dto)
        {
            try
            {
                // Bước 1: Service verify với Google (controller không biết ClientId)
                var payload = await _googleAuthService.VerifyGoogleTokenAsync(dto.IdToken);

                // Bước 2: GetOrCreate user trong DB
                var customer = await _googleAuthService
                    .GetOrCreateUserFromGoogleAsync(payload.Email, payload.Name);

                // Bước 3: Kiểm tra tài khoản có bị khoá không
                if (customer.IsActive != true)
                    return Unauthorized(new
                    {
                        success = false,
                        errorType = "AccountInactive",
                        message = "Your account has been deactivated. Please contact support."
                    });

                // Bước 4: Generate JWT — nhất quán với CustomerLogin
                var roles = new List<string> { "Customer" };
                var token = _jwtService.GenerateToken(
                    customer.CustomerId, customer.Email, roles);

                return Ok(new
                {
                    success = true,
                    message = "Google login successful",
                    token = token,
                    fullName = customer.FullName,
                    email = customer.Email
                });
            }
            catch (InvalidJwtException)
            {
                return Unauthorized(new
                {
                    success = false,
                    errorType = "InvalidGoogleToken",
                    message = "Invalid Google token. Please try again."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Server error: " + ex.Message
                });
            }
        }

        // POST: api/auth/google-callback
        [HttpPost("google-callback")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleCallback([FromBody] GoogleCallbackRequestDto dto)
        {
            try
            {
                // Bước 1: Exchange code → id_token
                var idToken = await _googleAuthService.ExchangeCodeForIdTokenAsync(
                    dto.Code, dto.RedirectUri);

                // Bước 2: Verify id_token
                var payload = await _googleAuthService.VerifyGoogleTokenAsync(idToken);

                // Bước 3: GetOrCreate user
                var customer = await _googleAuthService
                    .GetOrCreateUserFromGoogleAsync(payload.Email, payload.Name);

                // Bước 4: Kiểm tra active
                if (customer.IsActive != true)
                    return Unauthorized(new
                    {
                        success = false,
                        errorType = "AccountInactive",
                        message = "Your account has been deactivated. Please contact support."
                    });

                // Bước 5: Generate JWT
                var roles = new List<string> { "Customer" };
                var token = _jwtService.GenerateToken(
    customer.CustomerId, customer.Email, roles,
    customer.FullName ?? "");

                return Ok(new
                {
                    success = true,
                    message = "Google login successful",
                    token = token,
                    fullName = customer.FullName,
                    email = customer.Email
                });
            }
            catch (InvalidJwtException)
            {
                return Unauthorized(new
                {
                    success = false,
                    errorType = "InvalidGoogleToken",
                    message = "Invalid Google token. Please try again."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Token exchange failed: " + ex.Message
                });
            }
        }

        // POST: api/auth/forgot-password
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _forgotPasswordService.SendResetPasswordEmailAsync(dto.Email);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        // GET: api/auth/validate-reset-token?email=...&token=...
        [HttpGet("validate-reset-token")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateResetToken([FromQuery] string email, [FromQuery] string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
                return BadRequest(new { success = false, message = "Invalid request." });

            var result = await _forgotPasswordService.ValidateResetTokenAsync(email, token);

            if (!result.Valid)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        // POST: api/auth/reset-password
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _forgotPasswordService.ResetPasswordAsync(dto.Email, dto.Token, dto.NewPassword);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        [Authorize(Roles = "Customer")]
        [HttpGet("my-orders")]
        public IActionResult MyOrders()
        {
            return Ok("Danh sách đơn hàng của bạn");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-only")]
        public IActionResult AdminOnly()
        {
            return Ok("Chỉ admin mới vào được");
        }

        [Authorize(Roles = "Staff")]
        [HttpGet("staff-only")]
        public IActionResult StaffOnly()
        {
            return Ok("Chỉ staff mới vào được");
        }

        // hello

        [HttpGet("hash")]
        public IActionResult Hash(string password)
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(password);
            return Ok(hash);
        }
    }
}
