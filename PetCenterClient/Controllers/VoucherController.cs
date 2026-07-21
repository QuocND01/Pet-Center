using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;
using PetCenterClient.ViewModels.ManageVoucher;
using System.Net.Http;
using System.Text;

namespace PetCenterClient.Controllers
{
    public class VoucherController : Controller
    {
        private readonly IVoucherApiService _voucherService;

        public VoucherController(IVoucherApiService voucherService)
        {
            _voucherService = voucherService;
        }

        // ============================================================
        // HELPER
        // ============================================================
        private bool IsAuthorized(out IActionResult? redirect)
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || (role != "Admin" && role != "Sale Staff"))
            {
                redirect = RedirectToAction("Login", "Auth");
                return false;
            }
            redirect = null;
            return true;
        }

        // ============================================================
        // VOUCHER — VIEW LIST
        // ============================================================
        public IActionResult Index()
        {
            if (!IsAuthorized(out var redirect)) return redirect!;
            return View("~/Views/AdminViews/Voucher/Index.cshtml");
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            if (!IsAuthorized(out var redirect)) return redirect!;
            var vouchers = await _voucherService.GetAllAsync();
            return Json(vouchers);
        }

        // ============================================================
        // VOUCHER — GET BY ID
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> GetById(Guid id)
        {
            if (!IsAuthorized(out var redirect)) return redirect!;
            var voucher = await _voucherService.GetByIdAsync(id);
            if (voucher == null)
                return Json(new { success = false, message = "Voucher not found." });
            return Json(voucher);
        }

        // ============================================================
        // VOUCHER — CREATE
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateVoucherViewModel dto)
        {
            if (!IsAuthorized(out var redirect)) return redirect!;

            if (dto == null || !ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { message = errors.Any() ? string.Join(" | ", errors) : "Invalid data." });
            }

            try
            {
                var (success, message, data) = await _voucherService.CreateAsync(dto);
                if (!success)
                    return BadRequest(new { message });
                return Ok(new { message, data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Server error while creating voucher: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVoucherDto dto)
        {
            if (!IsAuthorized(out var redirect)) return redirect!;
            if (dto == null || id == Guid.Empty)
                return Json(new { success = false, message = "Invalid data." });

            var (success, message, data) = await _voucherService.UpdateAsync(id, dto);
            if (!success)
                return BadRequest(new { message });
            return Ok(new { message, data });
        }

        // ============================================================
        // VOUCHER — TOGGLE STATUS
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> Toggle(Guid id, bool isActive)
        {
            if (!IsAuthorized(out var redirect)) return redirect!;

            var (success, message) = await _voucherService.ToggleStatusAsync(id, isActive);
            if (!success)
                return BadRequest(new { message });
            return Ok(new { message });
        }
    }
}
