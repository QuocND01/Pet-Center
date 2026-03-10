using IdentityAPI.DTOs.Resquest;
using IdentityAPI.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize(Roles = "Admin")]
    public class StaffController : ControllerBase
    {
        private readonly IStaffService _staffService;

        public StaffController(IStaffService staffService) => _staffService = staffService;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _staffService.GetListAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var res = await _staffService.GetDetailsAsync(id);
            return res == null ? NotFound(new { message = "Staff not found" }) : Ok(res);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
            => Ok(await _staffService.SearchStaffAsync(keyword));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StaffCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _staffService.CreateStaffAsync(dto);

            if (!success)
            {
                // Trả về thông báo rõ ràng hơn cho client
                return BadRequest(new
                {
                    message = "Cannot create staff. Email may already exist or role 'Staff' is not configured in the database."
                });
            }

            return Ok(new { message = "Staff created successfully and assigned role 'Staff'." });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] StaffUpdateDto dto)
        {
            var success = await _staffService.UpdateStaffAsync(id, dto);
            return success
                ? Ok(new { message = "Staff updated successfully" })
                : NotFound(new { message = "Staff not found" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _staffService.DeleteStaffAsync(id);
            return Ok(new { message = "Staff deleted successfully" });
        }
    }
}