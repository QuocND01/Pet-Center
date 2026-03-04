using IdentityAPI.DTOs.Request;
using IdentityAPI.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize(Roles = "Admin")] // Mở ra nếu bạn muốn phân quyền
    public class StaffController : ControllerBase
    {
        private readonly IStaffService _staffService;
        public StaffController(IStaffService staffService) => _staffService = staffService;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _staffService.GetListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var res = await _staffService.GetDetailsAsync(id);
            return res == null ? NotFound() : Ok(res);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
            => Ok(await _staffService.SearchStaffAsync(keyword));

        [HttpPost]
        public async Task<IActionResult> Create(StaffCreateDto dto)
        {
            var success = await _staffService.CreateStaffAsync(dto);
            return success ? Ok("Staff created successfully") : BadRequest("Email already exists");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, StaffUpdateDto dto)
        {
            var success = await _staffService.UpdateStaffAsync(id, dto);
            return success ? Ok("Staff updated successfully") : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _staffService.DeleteStaffAsync(id);
            return Ok("Staff deleted successfully");
        }
    }
}