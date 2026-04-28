using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AttendanceAPI.DTOs;
using AttendanceAPI.Service.Interface;

namespace AttendanceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendancesController : ControllerBase
    {
        private readonly IAttendanceService _service;

        public AttendancesController(IAttendanceService service)
        {
            _service = service;
        }

        // Chức năng: View Attendance List For Admin, Search, Filter
        // [Authorize(Roles = "Admin,Staff")]
        [HttpGet]
        public async Task<IActionResult> GetAttendances([FromQuery] AttendanceQueryParameters query)
        {
            var result = await _service.GetAttendancesAsync(query);
            return Ok(result);
        }

        // Chức năng: View Attendance History For Staff
        // [Authorize(Roles = "Staff,Admin")]
        [HttpGet("staff/{staffId}")]
        public async Task<IActionResult> GetStaffHistory(Guid staffId, [FromQuery] AttendanceQueryParameters query)
        {
            // Ép buộc param StaffId bằng id truyền trên URL để chỉ lấy đúng dữ liệu của staff đó
            query.StaffId = staffId;
            var result = await _service.GetAttendancesAsync(query);
            return Ok(result);
        }

        // Chức năng: Add Attendance (Chỉ dành cho Admin tạo thủ công)
        // [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddAttendance([FromBody] AttendanceRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _service.CreateAttendanceAsync(dto);
            return Ok(new { message = "Thêm bản ghi điểm danh thành công.", data = result });
        }

        // Chức năng: Update Attendance Status (Chỉ Admin)
        // [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] int newStatus)
        {
            var success = await _service.UpdateAttendanceStatusAsync(id, newStatus);
            if (!success)
                return NotFound(new { message = "Không tìm thấy bản ghi điểm danh." });

            return Ok(new { message = "Cập nhật trạng thái điểm danh thành công." });
        }
    }
}