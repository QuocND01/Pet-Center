using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;
using System.Net.Http;
using System.Text;

namespace PetCenterClient.Controllers
{
    public class VoucherController : Controller
    {
        private readonly IVoucherService _service;

        public VoucherController(IVoucherService service)
        {
            _service = service;
        }
        public async Task<IActionResult> Index(string? code)
        {
            var vouchers = string.IsNullOrEmpty(code)
                ? await _service.GetAllAsync()
                : await _service.SearchAsync(code);

            return View("~/Views/AdminViews/Voucher/Index.cshtml", vouchers);
        }

        public async Task<IActionResult> Detail(Guid id)
        {
            var v = await _service.GetByIdAsync(id);
            if (v == null) return NotFound();

            return View("~/Views/AdminViews/Voucher/Detail.cshtml", v);
        }

        public IActionResult Create()
        {
            return View("~/Views/AdminViews/Voucher/Create.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateVoucherDTO dto)
        {
            if (!ModelState.IsValid)
                return View("~/Views/AdminViews/Voucher/Create.cshtml", dto);

            await _service.CreateAsync(dto);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var v = await _service.GetByIdAsync(id);
            if (v == null) return NotFound();

            return View("~/Views/AdminViews/Voucher/Edit.cshtml", v);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, CreateVoucherDTO dto)
        {
            await _service.UpdateAsync(id, dto);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Apply([FromBody] ApplyVoucherDTO dto)
        {
            var result = await _service.ApplyVoucherAsync(dto);
            return Json(result);
        }
    }
}