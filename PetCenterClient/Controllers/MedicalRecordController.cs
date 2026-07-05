using Microsoft.AspNetCore.Mvc;
using PetCenterClient.Services.Interface;
using static PetCenterClient.ViewModels.MedicalRecord.MedicalRecordViewModel;

namespace PetCenterClient.Controllers
{
    public class MedicalRecordController : Controller
    {
        private readonly IMedicalRecordAPIClient _service;
        private readonly IPrescriptionItemAPIClient _prescriptionService;

        public MedicalRecordController(
            IMedicalRecordAPIClient service,
            IPrescriptionItemAPIClient prescriptionService)
        {
            _service = service;
            _prescriptionService = prescriptionService;
        }

        // GET: Vet/Admin list screen
        public async Task<IActionResult> IndexAsync(
            string? search,
            int? status,
            int page = 1)
        {
            var result = await _service.GetAllAdminAsync(search, status, page);

            ViewBag.CurrentPage = result.CurrentPage;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.TotalCount = result.TotalCount;
            ViewBag.Search = search;
            ViewBag.Status = status;

            return View("~/Views/AdminViews/MedicalRecord/Index.cshtml", result.Data);
        }

        // GET: Customer list screen
        public async Task<IActionResult> CustomerIndexAsync(string? search)
        {
            var customerId = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerId))
                return RedirectToAction("Login", "Auth");

            var items = await _service.GetByCustomerAsync(Guid.Parse(customerId), search);
            ViewBag.Search = search;
            return View("~/Views/CustomerViews/MedicalRecord/Index.cshtml", items);
        }

        // GET: Details popup (shared)
        public async Task<IActionResult> DetailsAsync(Guid id)
        {
            var record = await _service.GetByIdAsync(id);
            if (record == null) return NotFound();
            return PartialView("~/Views/AdminViews/MedicalRecord/_Details.cshtml", record);
        }

        // GET: Customer details popup
        public async Task<IActionResult> CustomerDetailsAsync(Guid id)
        {
            var customerIdStr = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerIdStr))
                return RedirectToAction("Login", "Auth");

            var record = await _service.GetByIdAsync(id);
            if (record == null) return NotFound();

            if (record.CustomerId != Guid.Parse(customerIdStr) || record.Status != 2)
            {
                return Forbid();
            }

            return PartialView("~/Views/CustomerViews/MedicalRecord/_Details.cshtml", record);
        }

        // GET: Create popup
        public async Task<IActionResult> CreateAsync()
        {
            var appointments = await _service.GetCompletedAppointmentsAsync();
            ViewBag.CompletedAppointments = appointments;
            ViewBag.Diseases = await _service.GetDiseasesAsync(null);
            return PartialView("~/Views/AdminViews/MedicalRecord/_Create.cshtml",
                new CreateMedicalRecordViewModel());
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsync(CreateMedicalRecordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var appointments = await _service.GetCompletedAppointmentsAsync();
                ViewBag.CompletedAppointments = appointments;
                ViewBag.Diseases = await _service.GetDiseasesAsync(null);
                return PartialView("~/Views/AdminViews/MedicalRecord/_Create.cshtml", model);
            }

            try
            {
                await _service.CreateAsync(model);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var appointments = await _service.GetCompletedAppointmentsAsync();
                ViewBag.CompletedAppointments = appointments;
                ViewBag.Diseases = await _service.GetDiseasesAsync(null);
                return PartialView("~/Views/AdminViews/MedicalRecord/_Create.cshtml", model);
            }
        }

        // GET: Edit popup
        public async Task<IActionResult> EditAsync(Guid id)
        {
            var record = await _service.GetByIdAsync(id);
            if (record == null) return NotFound();

            if (record.Status == 2) // Completed
                return Json(new { error = "Cannot edit a completed medical record" });

            var model = new UpdateMedicalRecordViewModel
            {
                RecordId = record.RecordId,
                DiseaseId = record.DiseaseId,
                CustomDiseaseName = record.DiseaseId == null ? record.DiseaseNameSnapshot : null,
                Diagnosis = record.Diagnosis,
                Treatment = record.Treatment,
                Note = record.Note
            };
            ViewBag.PetSpecies = record.PetSpecies;
            ViewBag.Diseases = await _service.GetDiseasesAsync(null);
            return PartialView("~/Views/AdminViews/MedicalRecord/_Edit.cshtml", model);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAsync(Guid id, UpdateMedicalRecordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Diseases = await _service.GetDiseasesAsync(null);
                return PartialView("~/Views/AdminViews/MedicalRecord/_Edit.cshtml", model);
            }

            try
            {
                await _service.UpdateAsync(id, model);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Diseases = await _service.GetDiseasesAsync(null);
                return PartialView("~/Views/AdminViews/MedicalRecord/_Edit.cshtml", model);
            }
        }

        // GET: Change Status popup
        public async Task<IActionResult> ChangeStatusAsync(Guid id, int targetStatus)
        {
            var record = await _service.GetByIdAsync(id);
            if (record == null) return NotFound();

            var model = new ChangeStatusViewModel
            {
                RecordId = record.RecordId,
                Diagnosis = record.Diagnosis,
                CurrentStatus = record.Status ?? 1,
                CurrentStatusName = record.StatusName,
                TargetStatus = targetStatus,
                TargetStatusName = targetStatus switch
                {
                    1 => "Drafted",
                    2 => "Completed",
                    3 => "Cancelled",
                    _ => "Unknown"
                }
            };
            return PartialView("~/Views/AdminViews/MedicalRecord/_ChangeStatus.cshtml", model);
        }

        // POST: Change Status confirmed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatusConfirmedAsync(Guid id, int status)
        {
            try
            {
                await _service.ChangeStatusAsync(id, status);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
