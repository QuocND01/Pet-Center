using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Plugins;
using PetCenterAPI.DTOs.Requests.Login;
using PetCenterAPI.DTOs.Requests.Register;
using PetCenterAPI.DTOs.Responses;
using PetCenterAPI.DTOs.Responses.Login;
using PetCenterAPI.Security;
using PetCenterAPI.Service.Interface;

namespace PetCenterAPI.Controllers
{
    [ApiController]
    [Route("api/auths")]
    public class AuthsController : Controller
    {
        private readonly ICustomerAuthService _customerAuthService;
        // private readonly IGoogleAuthService _googleAuthService;
        private readonly IJwtService _jwtService;
        // private readonly IForgotPasswordService _forgotPasswordService;
        private readonly IStaffAuthService _staffAuthService;

        public AuthsController(
            ICustomerAuthService customerAuthService,
            // IGoogleAuthService googleAuthService,
            IJwtService jwtService,
            IStaffAuthService staffAuthService
            // IForgotPasswordService forgotPasswordService
            )
        {
            _customerAuthService = customerAuthService;
            // _googleAuthService = googleAuthService;
            _jwtService = jwtService;
            //_forgotPasswordService = forgotPasswordService;
            _staffAuthService = staffAuthService;
        }

        // ============================================================
        // LOGIN
        // ============================================================
        [HttpPost("customer-login")]
        [AllowAnonymous]
        public async Task<IActionResult> CustomerLogin([FromBody] LoginRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, token, errorType, message) =
                await _customerAuthService.LoginAsync(request.Email, request.Password);

            if (!success)
            {
                return Unauthorized(new LoginResponseDTO
                {
                    Success = false,
                    Message = message,
                    ErrorType = errorType,
                    Token = (string?)null
                });
            }

            return Ok(new LoginResponseDTO
            {
                Success = true,
                Message = "Login success",
                Token = token
            });
        }

        // ============================================================
        // LOGIN — STAFF / ADMIN
        // ============================================================
        [HttpPost("staff-login")]
        [AllowAnonymous]
        public async Task<IActionResult> StaffLogin([FromBody] StaffLoginRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, token, errorType, message, roles) =
                await _staffAuthService.LoginAsync(request.Email, request.Password);

            if (!success)
            {
                return Unauthorized(new
                {
                    success = false,
                    message,
                    errorType
                });
            }

            var primaryRole = roles.Contains("Admin") ? "Admin"
                : roles.Contains("Sale Staff") ? "Sale Staff"
                : roles.Contains("Inventory Staff") ? "Inventory Staff"
                : roles.Contains("Vet") ? "Vet"
                : null;

            return Ok(new
            {
                success = true,
                message,
                token,
                roles,
                primaryRole
            });
        }

        // ============================================================
        // REGISTER — STEP 1: Nhận form, tạo customer tạm, gửi OTP
        // ============================================================
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _customerAuthService.RegisterAsync(request);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        // ============================================================
        // REGISTER — STEP 2: Verify OTP, kích hoạt tài khoản
        // ============================================================
        [HttpPost("verify-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _customerAuthService.VerifyOtpAsync(request);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        // ============================================================
        // REGISTER — RESEND OTP
        // ============================================================
        [HttpPost("resend-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _customerAuthService.ResendOtpAsync(request.Email);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }
    }
}
