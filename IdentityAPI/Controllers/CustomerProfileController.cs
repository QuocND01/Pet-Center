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
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null)
                return Unauthorized("Invalid token");

            var userId = Guid.Parse(userIdClaim);

            var result = await _customerService.UpdateProfileAsync(userId, request);

            if (!result)
                return NotFound("Customer not found");

            return Ok(new
            {
                status = 200,
                message = "Profile updated successfully"
            });
        }
    }
}
