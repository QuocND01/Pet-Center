using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using CustomerAPI.Service.Interface;
using CustomerAPI.DTOs.Request;

namespace CustomerAPI.Controllers
{
    [ApiController]
    [Route("api/customer")]
    [Authorize(Roles = "Customer")]
    public class CustomerProfileController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerProfileController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        // GET: api/customer/profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (customerIdClaim == null || !Guid.TryParse(customerIdClaim, out var customerId))
                return Unauthorized(new { success = false, message = "Invalid token." });

            var profile = await _customerService.GetProfileAsync(customerId);
            if (profile == null)
                return NotFound(new { success = false, message = "Customer not found." });

            return Ok(new { status = 200, message = "Get profile successfully", data = profile });
        }

        // PUT: api/customer/profile
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateCustomerProfileRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                return BadRequest(new { success = false, message = string.Join("; ", errors) });
            }

            var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(customerIdClaim) || !Guid.TryParse(customerIdClaim, out var customerId))
                return Unauthorized(new { success = false, message = "Invalid token." });

            var result = await _customerService.UpdateProfileAsync(customerId, request);
            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }
    }
}
