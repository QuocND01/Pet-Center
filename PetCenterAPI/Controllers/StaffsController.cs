using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCenterAPI.DTOs.Requests.ManageStaff;
using PetCenterAPI.Service.Interface;

namespace PetCenterAPI.Controllers
{
    [ApiController]
    [Route("api/staff")]
    [Authorize(Roles = "Admin")]
    public class StaffsController : ControllerBase
    {
        private readonly IStaffService _staffService;

        public StaffsController(IStaffService staffService)
        {
            _staffService = staffService;
        }

        // ============================================================
        // STAFF — VIEW LIST
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _staffService.GetAllAsync();
            return Ok(result);
        }

        // ============================================================
        // STAFF — ASSIGNABLE ROLES (dropdown)
        // ============================================================
        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            var result = await _staffService.GetAssignableRolesAsync();
            return Ok(result);
        }

        // ============================================================
        // STAFF — VIEW DETAIL
        // ============================================================
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _staffService.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { success = false, message = "Staff not found." });

            return Ok(result);
        }

        // ============================================================
        // STAFF — CREATE
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateStaffRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = CollectErrors() });

            var (success, message, staffId) = await _staffService.CreateAsync(request);
            if (!success)
                return BadRequest(new { success = false, message });

            return CreatedAtAction(nameof(GetById), new { id = staffId },
                new { success = true, message, data = new { staffId } });
        }

        // ============================================================
        // STAFF — UPDATE
        // ============================================================
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] UpdateStaffRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = CollectErrors() });

            var (success, message) = await _staffService.UpdateAsync(id, request);
            if (!success)
            {
                return message == "Staff not found."
                    ? NotFound(new { success = false, message })
                    : BadRequest(new { success = false, message });
            }

            return Ok(new { success = true, message });
        }

        // ============================================================
        // STAFF — SOFT DELETE
        // ============================================================
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var (success, message) = await _staffService.DeleteAsync(id);
            if (!success)
            {
                return message == "Staff not found."
                    ? NotFound(new { success = false, message })
                    : BadRequest(new { success = false, message });
            }

            return Ok(new { success = true, message });
        }

        // ============================================================
        // HELPER
        // ============================================================
        private string CollectErrors()
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .ToList();
            return string.Join(" | ", errors);
        }
    }
}
