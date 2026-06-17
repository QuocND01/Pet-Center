using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCenterAPI.DTOs.Requests.ManageVoucher;
using PetCenterAPI.Service.Interface;

namespace PetCenterAPI.Controllers
{
    [ApiController]
    [Route("api/voucher")]
    public class VouchersController : ControllerBase
    {
        private readonly IVoucherService _voucherService;

        public VouchersController(IVoucherService voucherService)
        {
            _voucherService = voucherService;
        }

        // ============================================================
        // VOUCHER — VIEW LIST
        // ============================================================
        [HttpGet("vouchers")]
        [Authorize(Roles = "Admin,Sale Staff")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _voucherService.GetAllAsync();
            return Ok(result);
        }

        // ============================================================
        // VOUCHER — GET BY ID
        // ============================================================
        [HttpGet("vouchers/{id:guid}")]
        [Authorize(Roles = "Admin,Sale Staff")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _voucherService.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { message = "Voucher not found." });

            return Ok(result);
        }

        // ============================================================
        // VOUCHER — CREATE
        // ============================================================
        [HttpPost("vouchers")]
        [Authorize(Roles = "Admin,Sale Staff")]
        public async Task<IActionResult> Create([FromBody] CreateVoucherRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { message = string.Join(" | ", errors) });
            }

            var (success, message, data) = await _voucherService.CreateAsync(request);

            if (!success)
                return BadRequest(new { message });

            return CreatedAtAction(nameof(GetById), new { id = data!.VoucherId }, new { message, data });
        }

        // ============================================================
        // VOUCHER — TOGGLE STATUS
        // ============================================================
        [HttpPatch("vouchers/{id:guid}/toggle")]
        [Authorize(Roles = "Admin,Sale Staff")]
        public async Task<IActionResult> Toggle(Guid id, [FromQuery] bool isActive)
        {
            var (success, message) = await _voucherService.ToggleStatusAsync(id, isActive);

            if (!success)
                return BadRequest(new { message });

            return Ok(new { message });
        }
    }
}
