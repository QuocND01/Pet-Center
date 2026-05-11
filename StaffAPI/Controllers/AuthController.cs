using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffAPI.DTOs.Auth;
using StaffAPI.Services.Interfaces;

namespace StaffAPI.Controllers
{
    [ApiController]
    [Route("api/staff/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IStaffAuthService _staffAuthService;

        public AuthController(IStaffAuthService staffAuthService)
        {
            _staffAuthService = staffAuthService;
        }

        // POST: api/auth/staff-login
        [HttpPost("staff-login")]
        [AllowAnonymous]
        public async Task<IActionResult> StaffLogin([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, token, errorType, message, roles) =
                await _staffAuthService.LoginAsync(dto.Email, dto.Password);

            if (!success)
                return Unauthorized(new { success = false, message, errorType });

            var primaryRole = roles.Contains("Admin") ? "Admin"
        : roles.Contains("Sale Staff") ? "Sale Staff"
        : roles.Contains("Inventory Staff") ? "Inventory Staff"
        : roles.Contains("Vet") ? "Vet"
        : null;

            return Ok(new { success = true, message, token, roles, primaryRole });
        }
    }
}
