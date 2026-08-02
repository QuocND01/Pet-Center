using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PetCenterAPI.DTOs.Requests.ManageCustomer;
using PetCenterAPI.Hubs;
using PetCenterAPI.Service.Interface;

namespace PetCenterAPI.Controllers
{
    [Route("api/customers")]
    [ApiController]
    public class CustomersManagementController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IHubContext<AppHub> _hubContext;

        public CustomersManagementController(ICustomerService customerService, IHubContext<AppHub> hubContext)
        {
            _customerService = customerService;
            _hubContext = hubContext;
        }

        // ============================================================
        // STAFF / ADMIN — VIEW LIST CUSTOMER
        // ============================================================
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

        // ============================================================
        // STAFF / ADMIN — VIEW DETAIL CUSTOMER
        // ============================================================
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

        // ============================================================
        // STAFF / ADMIN — CHANGE STATUS CUSTOMER
        // ============================================================
        [HttpPut("{id:guid}/status")]
        [Authorize(Roles = "Admin,Sale Staff")]
        public async Task<IActionResult> ChangeCustomerStatus(
            Guid id, [FromBody] ChangeCustomerStatusRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _customerService.ChangeCustomerStatusAsync(id, request.IsActive);

            if (!result)
                return NotFound(new { status = 404, message = "Customer not found" });

            if (!request.IsActive)
            {
                var payload = new
                {
                    customerId = id.ToString().ToLower(),
                    message = "Your account has been deactivated by an administrator.",
                    reason = "Account Status Deactivated",
                    timestamp = DateTime.Now
                };

                // Gửi đến đúng user bị block (theo UserId claim trong JWT)
                await _hubContext.Clients.User(id.ToString()).SendAsync("AccountDeactivated", payload);
                // Gửi theo Group (backup — đảm bảo bắt được kể cả khi UserIdentifier mapping lệch)
                await _hubContext.Clients.Group(id.ToString().ToLower()).SendAsync("AccountDeactivated", payload);
                // KHÔNG dùng Clients.All — tránh broadcast thừa tới toàn bộ user đang online
            }

            return Ok(new
            {
                status = 200,
                message = $"Customer status changed to {(request.IsActive ? "Active" : "Inactive")} successfully"
            });
        }
    }
}
