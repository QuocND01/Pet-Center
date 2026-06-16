using Microsoft.AspNetCore.Mvc;
using PetCenterClient.ViewModels;
using PetCenterClient.Services.Interface;
using PetCenterClient.ViewModels.Supplier;

namespace PetCenterClient.Controllers
{
    public class SupplierController : Controller
    {
        private readonly ISupplierApiService _supplierService;

        public SupplierController(ISupplierApiService supplierService)
        {
            _supplierService = supplierService;
        }

        // GET: Suppliers
        public async Task<IActionResult> Index()
        {
            var suppliers = await _supplierService.GetAllAsync();

            if (suppliers == null)
                suppliers = new List<ViewSupplierViewModel>();

            return View("~/Views/AdminViews/Supplier/Index.cshtml", suppliers);
        }

        // GET: Create
        public IActionResult Create()
        {   
            
            return View("~/Views/AdminViews/Supplier/Create.cshtml");

        }

        // POST: Create
        [HttpPost]
        public async Task<IActionResult> Create(CreateSupplierViewModel dto)
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
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var supplier = await _supplierService.GetByIdAsync(id);

            if (supplier == null)
                return NotFound();

            var dto = new CreateSupplierViewModel
            {
                SupplierId = id,
                TaxId = supplier.TaxId,
                SupplierName = supplier.SupplierName,
                SupplierEmail = supplier.SupplierEmail,
                SupplierPhoneNumber = supplier.SupplierPhoneNumber,
                SupplierAddress = supplier.SupplierAddress,
                ContactPerson = supplier.ContactPerson,
                SupplierDescription = supplier.SupplierDescription
            };

            return PartialView("~/Views/AdminViews/Supplier/Edit.cshtml", dto);
        }

        // POST: Edit
        [HttpPost]
        public async Task<IActionResult> Edit(CreateSupplierViewModel dto)
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