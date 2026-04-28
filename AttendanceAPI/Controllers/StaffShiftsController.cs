using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AttendanceAPI.DTOs;
using AttendanceAPI.Service.Interface;

namespace AttendanceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StaffShiftsController : ControllerBase
    {
        private readonly IStaffShiftService _service;

        public StaffShiftsController(IStaffShiftService service)
        {
            _service = service;
        }

        // 1 & 2 & 3: View List, Filter, Search (Admin & All Staff)
        // Bỏ comment dòng [Authorize] khi bạn ráp Token JWT
        // [Authorize(Roles = "Admin,Staff")] 
        [HttpGet]
        public async Task<IActionResult> GetShifts([FromQuery] StaffShiftQueryParameters query)
        {
            var result = await _service.GetShiftsAsync(query);
            return Ok(result);
        }

        // 4. View Shift Details (Admin & All Staff)
        // [Authorize(Roles = "Admin,Staff")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetails(Guid id)
        {
            var shift = await _service.GetShiftDetailsAsync(id);
            if (shift == null) return NotFound(new { message = "Không tìm thấy thông tin ca làm việc." });

            return Ok(shift);
        }

        // 5. Add Staff Shift (Admin Only)
        // [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateShift([FromBody] StaffShiftRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _service.CreateShiftAsync(dto);
            return CreatedAtAction(nameof(GetDetails), new { id = result.ShiftId }, result);
        }

        // 6. Change Shift Status (Admin Only)
        // [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] int newStatus)
        {
            var success = await _service.ChangeShiftStatusAsync(id, newStatus);
            if (!success) return NotFound(new { message = "Không tìm thấy ca làm việc để thay đổi trạng thái." });

            return Ok(new { message = "Thay đổi trạng thái ca làm việc thành công." });
        }
    }
}