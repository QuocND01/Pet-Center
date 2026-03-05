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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetAddressByIdAsync(id);
            return result == null ? NotFound("Không tìm thấy địa chỉ") : Ok(result);
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
            return success ? NoContent() : NotFound("Cập nhật thất bại hoặc không tìm thấy ID");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAddressAsync(id);
            return success ? Ok("Xóa thành công") : NotFound("Không tìm thấy địa chỉ để xóa");
        }
    }
}
