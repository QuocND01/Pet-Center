using Microsoft.AspNetCore.Mvc;
using PetCenterClient.Services.Interface;
using static PetCenterClient.ViewModels.PrescriptionItem.PrescriptionItemViewModel;

namespace PetCenterClient.Controllers
{
    public class PrescriptionItemController : Controller
    {
        private readonly IPrescriptionItemAPIClient _service;

        public PrescriptionItemController(IPrescriptionItemAPIClient service)
        {
            _service = service;
        }

        // GET: Create popup
        public IActionResult CreateAsync(Guid recordId)
        {
            var model = new CreatePrescriptionItemViewModel { RecordId = recordId };
            return PartialView("~/Views/AdminViews/PrescriptionItem/_Create.cshtml", model);
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsync(CreatePrescriptionItemViewModel model)
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
        public async Task<IActionResult> DetailsAsync(Guid id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return PartialView("~/Views/AdminViews/PrescriptionItem/_Details.cshtml", item);
        }

        // GET: Edit popup
        public async Task<IActionResult> EditAsync(Guid id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();

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
        public async Task<IActionResult> EditAsync(Guid id, UpdatePrescriptionItemViewModel model)
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
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();

            var model = new DeletePrescriptionItemViewModel
            {
                PrescriptionItemId = item.PrescriptionItemId,
                RecordId = item.RecordId,
                MedicineName = item.MedicineName,
                Status = item.Status
            };
            return PartialView("~/Views/AdminViews/PrescriptionItem/_Delete.cshtml", model);
        }

        // POST: Delete confirmed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmedAsync(Guid id)
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

        // POST: Change to Complete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteAsync(Guid id)
        {
            try
            {
                await _service.ChangeStatusAsync(id, 2); // 2 = Complete
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
