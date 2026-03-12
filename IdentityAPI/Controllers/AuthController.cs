using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IdentityAPI.DTOs;
using IdentityAPI.Service.Interface;
using IdentityAPI.DTOs.Resquest;

namespace IdentityAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ICustomerAuthService _customerAuthService;
        private readonly IStaffAuthService _staffAuthService;

        public AuthController(ICustomerAuthService customerAuthService, IStaffAuthService staffAuthService)
        {
            _customerAuthService = customerAuthService;
            _staffAuthService = staffAuthService;
        }

        // POST: api/auth/customer-login
        [HttpPost("customer-login")]
        [AllowAnonymous]
        public async Task<IActionResult> CustomerLogin([FromBody] LoginDto dto)
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

        // POST: api/auth/staff-login
        [HttpPost("staff-login")]
        [AllowAnonymous]
        public async Task<IActionResult> StaffLogin([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var token = await _staffAuthService.LoginAsync(dto.Email, dto.Password);

            if (token == null)
            {
                return Unauthorized(new
                {
                    message = "Email or password incorrect"
                });
            }

            return Ok(new
            {
                message = "Login success",
                token
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
