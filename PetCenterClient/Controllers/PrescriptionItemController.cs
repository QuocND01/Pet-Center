using Microsoft.AspNetCore.Mvc;
using PetCenterClient.Services.Interface;
using static PetCenterClient.ViewModels.PrescriptionItem.PrescriptionItemViewModel;

namespace PetCenterClient.Controllers
{
    public class PrescriptionItemController : Controller
    {
        private readonly IPrescriptionItemAPIClient _service;
        private readonly IMedicalRecordAPIClient _medicalRecordService;

        public PrescriptionItemController(
            IPrescriptionItemAPIClient service,
            IMedicalRecordAPIClient medicalRecordService)
        {
            _service = service;
            _medicalRecordService = medicalRecordService;
        }

        // GET: Create popup
        public async Task<IActionResult> Create(Guid recordId)
        {
            var record = await _medicalRecordService.GetByIdAsync(recordId);
            if (record == null || record.Status != 1)
            {
                return Content("<div class='alert alert-danger m-3'>Cannot add prescription items. The medical record is completed or cancelled.</div>");
            }

            var model = new CreatePrescriptionItemViewModel { RecordId = recordId };
            return PartialView("~/Views/AdminViews/PrescriptionItem/_Create.cshtml", model);
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePrescriptionItemViewModel model)
        {
            if (!ModelState.IsValid)
                return PartialView("~/Views/AdminViews/PrescriptionItem/_Create.cshtml", model);

            try
            {
                await _service.CreateAsync(model);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return PartialView("~/Views/AdminViews/PrescriptionItem/_Create.cshtml", model);
            }
        }

        // GET: Details popup
        public async Task<IActionResult> Details(Guid id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return PartialView("~/Views/AdminViews/PrescriptionItem/_Details.cshtml", item);
        }

        // GET: Edit popup
        public async Task<IActionResult> Edit(Guid id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();

            var record = await _medicalRecordService.GetByIdAsync(item.RecordId);
            if (record == null || record.Status != 1)
            {
                return Content("<div class='alert alert-danger m-3'>Cannot edit this prescription item. The medical record is completed or cancelled.</div>");
            }

            var model = new UpdatePrescriptionItemViewModel
            {
                PrescriptionItemId = item.PrescriptionItemId,
                MedicineName = item.MedicineName,
                Dosage = item.Dosage,
                Duration = item.Duration,
                Quantity = item.Quantity,
                Note = item.Note
            };
            return PartialView("~/Views/AdminViews/PrescriptionItem/_Edit.cshtml", model);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UpdatePrescriptionItemViewModel model)
        {
            if (!ModelState.IsValid)
                return PartialView("~/Views/AdminViews/PrescriptionItem/_Edit.cshtml", model);

            try
            {
                await _service.UpdateAsync(id, model);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return PartialView("~/Views/AdminViews/PrescriptionItem/_Edit.cshtml", model);
            }
        }

        // GET: Delete popup
        public async Task<IActionResult> Delete(Guid id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();

            var record = await _medicalRecordService.GetByIdAsync(item.RecordId);
            if (record == null || record.Status != 1)
            {
                return Content("<div class='alert alert-danger m-3'>Cannot delete this prescription item. The medical record is completed or cancelled.</div>");
            }

            var model = new DeletePrescriptionItemViewModel
            {
                PrescriptionItemId = item.PrescriptionItemId,
                RecordId = item.RecordId,
                MedicineName = item.MedicineName
            };
            return PartialView("~/Views/AdminViews/PrescriptionItem/_Delete.cshtml", model);
        }

        // POST: Delete confirmed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
