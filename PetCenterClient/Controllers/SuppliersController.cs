using Microsoft.AspNetCore.Mvc;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Controllers
{
    public class SuppliersController : Controller
    {
        private readonly ISupplierService _supplierService;

        public SuppliersController(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        // GET: Suppliers
        public async Task<IActionResult> Index()
        {
            var suppliers = await _supplierService.GetAllAsync();

            if (suppliers == null)
                suppliers = new List<ReadSupplierDto>();

            return View("~/Views/AdminViews/Supplier/Index.cshtml", suppliers);
        }

        // GET: Create
        public IActionResult Create()
        {
            return View("~/Views/AdminViews/Supplier/Create.cshtml");
        }

        // POST: Create
        [HttpPost]
        public async Task<IActionResult> Create(CreateSupplierDto dto)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("~/Views/AdminViews/Supplier/Create.cshtml", dto);
            }

            await _supplierService.CreateAsync(dto);

            return Json(new { success = true });
        }

        // GET: Edit
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var supplier = await _supplierService.GetByIdAsync(id);

            if (supplier == null)
                return NotFound();

            return PartialView("~/Views/AdminViews/Supplier/Edit.cshtml", supplier);
        }

        // POST: Edit
        [HttpPost]
        public async Task<IActionResult> Edit(UpdateSupplierDto dto)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("~/Views/AdminViews/Supplier/Edit.cshtml", dto);
            }

            var result = await _supplierService.UpdateAsync(dto.SupplierId, dto);

            if (result)
                return Json(new { success = true });

            ModelState.AddModelError("", "Error updating supplier");

            return PartialView("~/Views/AdminViews/Supplier/Edit.cshtml", dto);
        }

        // DELETE
        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _supplierService.DeleteAsync(id);

            if (result)
                return Json(new { success = true });

            return BadRequest();
        }
    }
}