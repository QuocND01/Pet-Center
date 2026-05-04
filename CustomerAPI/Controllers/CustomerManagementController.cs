using CustomerAPI.DTOs.Request;
using CustomerAPI.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerAPI.Controllers
{
    [Route("api/customers")]
    [ApiController]
    public class CustomerManagementController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerManagementController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet("internal/{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetInternal(Guid id)
        {
            var result = await _customerService.GetInternalAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // GET: api/customers
        // ✅ JWT của StaffAPI dùng key khác → cần cấu hình thêm (xem phần JWT bên dưới)
        [HttpGet]
        [Authorize(Roles = "Admin,Sale Staff")]
        public async Task<IActionResult> GetAllCustomers()
        {
            var result = await _customerService.GetAllCustomersAsync();
            return Ok(new
            {
                status = 200,
                message = "Get customer list successfully",
                data = result
            });
        }

        // GET: api/customers/{id}
        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Admin,Sale Staff")]
        public async Task<IActionResult> GetCustomerById(Guid id)
        {
            var result = await _customerService.GetCustomerByIdAsync(id);
            if (result == null)
                return NotFound(new { status = 404, message = "Customer not found" });

            return Ok(new
            {
                status = 200,
                message = "Get customer detail successfully",
                data = result
            });
        }

        // PUT: api/customers/{id}/status — Admin only
        [HttpPut("{id:guid}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeCustomerStatus(
            Guid id, [FromBody] ChangeCustomerStatusRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _customerService.ChangeCustomerStatusAsync(id, request.IsActive);
            if (!result)
                return NotFound(new { status = 404, message = "Customer not found" });

            return Ok(new
            {
                status = 200,
                message = $"Customer status changed to {(request.IsActive ? "Active" : "Inactive")} successfully"
            });
        }

        // GET: api/customers/{id}/display-name
        // Public endpoint - chỉ trả về tên hiển thị, không cần Authorize
        [HttpGet("{id:guid}/display-name")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDisplayName(Guid id)
        {
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(id);
                if (customer == null)
                    return Ok(new { success = true, displayName = "Anonymous" });
                return Ok(new { success = true, displayName = customer.FullName });
            }
            catch
            {
                return Ok(new { success = true, displayName = "Anonymous" });
            }
        }
    }
}
