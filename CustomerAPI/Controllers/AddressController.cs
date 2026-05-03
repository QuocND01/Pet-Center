using CustomerAPI.DTOs.Address;
using CustomerAPI.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CustomerAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _service;

        public AddressController(IAddressService service)
        {
            _service = service;
        }

        // giả sử bạn lấy userId từ token
        private Guid GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (claim == null)
                throw new UnauthorizedAccessException("Cant find User profile");

            return Guid.Parse(claim.Value);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyAddresses()
        {
            var userId = GetUserId();
            var data = await _service.GetMyAddressesAsync(userId);
            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAddressDto dto)
        {
            var userId = GetUserId();
            var result = await _service.CreateAsync(userId, dto);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateAddressDto dto)
        {
            var success = await _service.UpdateAsync(id, dto);
            return success ? Ok() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAsync(id);
            return success ? Ok() : NotFound();
        }
    }
}
