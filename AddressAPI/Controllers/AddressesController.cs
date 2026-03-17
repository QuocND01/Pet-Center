using AddressAPI.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AddressAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AddressesController : ControllerBase
    {
        private readonly IAddressService _service;
        public AddressesController(IAddressService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAddressesAsync());

        // GET: api/Addresses/customer/{customerId}
        // Trả về tất cả address active của 1 customer
        [HttpGet("customer/{customerId:guid}")]
        public async Task<IActionResult> GetByCustomerId(Guid customerId)
        {
            var result = await _service.GetAddressesByCustomerIdAsync(customerId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetAddressByIdAsync(id);
            return result == null ? NotFound("Address not found") : Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(AddressCreateDTO dto)
        {
            var result = await _service.CreateAddressAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.AddressId }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, AddressCreateDTO dto)
        {
            var success = await _service.UpdateAddressAsync(id, dto);
            return success ? NoContent() : NotFound("Update failed or ID not found");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAddressAsync(id);
            return success ? Ok("Deleted successfully") : NotFound("Address not found");
        }
    }
}