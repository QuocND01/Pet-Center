using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCenterAPI.Service.Interface;

namespace PetCenterAPI.Controllers
{
    [Route("api/customers")]
    [ApiController]
    public class CustomersManagementController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersManagementController(ICustomerService customerService)
        {
            _customerService = customerService;
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
    }
}
