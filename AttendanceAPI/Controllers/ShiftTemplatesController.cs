using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AttendanceAPI.DTOs;
using AttendanceAPI.Service.Interface;

namespace AttendanceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShiftTemplatesController : ControllerBase
    {
        private readonly IShiftTemplateService _service;

        public ShiftTemplatesController(IShiftTemplateService service)
        {
            _service = service;
        }

        // Chức năng: View List & Search
        // [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetTemplates([FromQuery] ShiftTemplateQueryParameters query)
        {
            var result = await _service.GetTemplatesAsync(query);
            return Ok(result);
        }

        // Chức năng: View Details
        // [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetails(Guid id)
        {
            var template = await _service.GetTemplateDetailsAsync(id);
            if (template == null) return NotFound(new { message = "Không tìm thấy ca mẫu." });

            return Ok(template);
        }

        // Chức năng: Add Template
        // [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateTemplate([FromBody] ShiftTemplateRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _service.CreateTemplateAsync(dto);
            return CreatedAtAction(nameof(GetDetails), new { id = result.TemplateId }, result);
        }

        // Chức năng: Update Template
        // [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTemplate(Guid id, [FromBody] ShiftTemplateRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _service.UpdateTemplateAsync(id, dto);
            if (!success) return NotFound(new { message = "Không tìm thấy ca mẫu để cập nhật." });

            return Ok(new { message = "Cập nhật ca mẫu thành công." });
        }

        // Chức năng: Delete Template
        // [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTemplate(Guid id)
        {
            var success = await _service.DeleteTemplateAsync(id);
            if (!success) return NotFound(new { message = "Không tìm thấy ca mẫu để xóa." });

            return Ok(new { message = "Đã ngưng hoạt động ca mẫu này." });
        }
    }
}