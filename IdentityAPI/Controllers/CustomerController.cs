using IdentityAPI.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityAPI.Controllers
{
    [Route("api/customers")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        // Chỉ Admin hoặc Staff được xem
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet]
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

        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetCustomerById(Guid id)
        {
            var result = await _customerService.GetCustomerByIdAsync(id);

            if (result == null)
            {
                return NotFound(new
                {
                    status = 404,
                    message = "Customer not found"
                });
            }

            return Ok(new
            {
                status = 200,
                message = "Get customer detail successfully",
                data = result
            });
        }
    }
}
