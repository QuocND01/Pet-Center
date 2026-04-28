using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayrollAPI.DTOs;
using PayrollAPI.Service.Interface;

namespace PayrollAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ViolationsController : ControllerBase
    {
        private readonly IViolationService _service;

        public ViolationsController(IViolationService service)
        {
            _service = service;
        }

        // Chức năng: View List (Admin), Search, Filter
        // [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetViolations([FromQuery] ViolationQueryParameters query)
        {
            var result = await _service.GetViolationsAsync(query);
            return Ok(result);
        }

        // Chức năng: View History For Staff
        // [Authorize(Roles = "Staff,Admin")]
        [HttpGet("staff/{staffId}")]
        public async Task<IActionResult> GetStaffHistory(Guid staffId, [FromQuery] ViolationQueryParameters query)
        {
            query.StaffId = staffId; // Ép ID để staff không xem được vi phạm của người khác
            var result = await _service.GetViolationsAsync(query);
            return Ok(result);
        }

        // Chức năng: View Details
        // [Authorize(Roles = "Admin,Staff")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetails(Guid id)
        {
            var violation = await _service.GetViolationDetailsAsync(id);
            if (violation == null) return NotFound(new { message = "Không tìm thấy biên bản vi phạm." });

            return Ok(violation);
        }

        // Chức năng: Add Violations
        // [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateViolation([FromBody] ViolationRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _service.CreateViolationAsync(dto);
            return CreatedAtAction(nameof(GetDetails), new { id = result.ViolationId }, result);
        }

        // Chức năng: Change Violation Status
        // [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] int newStatus)
        {
            var success = await _service.ChangeViolationStatusAsync(id, newStatus);
            if (!success) return NotFound(new { message = "Không tìm thấy biên bản vi phạm để cập nhật." });

            return Ok(new { message = "Cập nhật trạng thái vi phạm thành công." });
        }
    }
}