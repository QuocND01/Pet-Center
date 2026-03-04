using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IdentityAPI.DTOs;
using IdentityAPI.Service.Interface;

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

            var token = await _customerAuthService.LoginAsync(dto.Email, dto.Password);

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
    }
}
