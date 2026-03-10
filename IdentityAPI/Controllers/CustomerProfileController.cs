using IdentityAPI.DTOs.Resquest;
using IdentityAPI.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IdentityAPI.Controllers
{
    [ApiController]
    [Route("api/customer")]
    [Authorize]
    public class CustomerProfileController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerProfileController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet("profile")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetProfile()
        {
            var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (customerIdClaim == null)
                return Unauthorized("Invalid token.");

            var customerId = Guid.Parse(customerIdClaim);

            var profile = await _customerService.GetProfileAsync(customerId);

            if (profile == null)
                return NotFound("Customer not found.");

            return Ok(new
            {
                status = 200,
                message = "Get profile successfully",
                data = profile
            });
        }

        [Authorize(Roles = "Customer")]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateCustomerProfileRequestDto request)
        {
            // ✅ Validate ModelState
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                var errorMessage = string.Join("; ", errors.Select(e => e.ErrorMessage));
                return BadRequest(new
                {
                    status = 400,
                    message = "Validation failed",
                    errors = errorMessage
                });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(new { status = 401, message = "Invalid token" });

            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { status = 401, message = "Invalid token format" });

            try
            {
                var result = await _customerService.UpdateProfileAsync(userId, request);
                if (!result)
                    return NotFound(new { status = 404, message = "Customer not found" });

                return Ok(new
                {
                    status = 200,
                    message = "Profile updated successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = 500,
                    message = "An error occurred while updating profile",
                    error = ex.Message
                });
            }
        }
    }
}
