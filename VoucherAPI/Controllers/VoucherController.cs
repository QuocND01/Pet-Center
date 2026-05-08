using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoucherAPI.DTOs.Request;
using VoucherAPI.Services.Intterfaces;

namespace VoucherAPI.Controllers
{
    [ApiController]
    [Route("api/voucher")]
    public class VoucherController : ControllerBase
    {
        private readonly IVoucherService _service;

        public VoucherController(IVoucherService service)
        {
            _service = service;
        }

        // ── GET ALL ──────────────────────────────────────────────
        // Admin, Sale Staff đều xem được
        [HttpGet("vouchers")]
        [Authorize(Roles = "Admin,Sale Staff")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        // ── GET BY ID ────────────────────────────────────────────
        [HttpGet("vouchers/{id:guid}")]
        [Authorize(Roles = "Admin,Sale Staff")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { message = "Voucher not found." });

            return Ok(result);
        }

        // ── GET BY CODE (public — dùng khi customer áp voucher ở checkout) ──
        [HttpGet("vouchers/code/{code}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return BadRequest(new { message = "Code is required." });

            var result = await _service.GetByCodeAsync(code);
            if (result == null)
                return NotFound(new { message = "Voucher not found." });

            // Nếu không active hoặc hết hạn → báo không hợp lệ
            if (result.IsActive == false)
                return BadRequest(new { message = "This voucher is no longer active." });

            if (result.ExpiredDate.HasValue && result.ExpiredDate.Value <= DateTime.UtcNow)
                return BadRequest(new { message = "This voucher has expired." });

            if (result.UseageLimit.HasValue && result.UsedCount >= result.UseageLimit.Value)
                return BadRequest(new { message = "This voucher has reached its usage limit." });

            return Ok(result);
        }

        // ── CREATE ───────────────────────────────────────────────
        [HttpPost("vouchers")]
        [Authorize(Roles = "Admin,Sale Staff")]
        public async Task<IActionResult> Create([FromBody] CreateVoucherDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { message = string.Join(" | ", errors) });
            }

            var (success, message, data) = await _service.CreateAsync(dto);
            if (!success)
                return BadRequest(new { message });

            return CreatedAtAction(nameof(GetById), new { id = data!.VoucherId },
                new { message, data });
        }

        // ── UPDATE ───────────────────────────────────────────────
        [HttpPut("vouchers/{id:guid}")]
        [Authorize(Roles = "Admin,Sale Staff")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVoucherDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { message = string.Join(" | ", errors) });
            }

            var (success, message, data) = await _service.UpdateAsync(id, dto);
            if (!success)
                return BadRequest(new { message });

            return Ok(new { message, data });
        }

        // ── TOGGLE STATUS ────────────────────────────────────────
        [HttpPatch("vouchers/{id:guid}/toggle")]
        [Authorize(Roles = "Admin,Sale Staff")]
        public async Task<IActionResult> Toggle(Guid id, [FromQuery] bool isActive)
        {
            var (success, message) = await _service.ToggleStatusAsync(id, isActive);
            if (!success)
                return BadRequest(new { message });

            return Ok(new { message });
        }
    }
}
