using Microsoft.AspNetCore.Mvc;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly IFeedbackService _service;

        public FeedbackController(IFeedbackService service)
        {
            _service = service;
        }

        //Admin
        public async Task<IActionResult> AdminList()
        {
            var feedbacks = await _service.GetAllAsync();

            return View("~/Views/AdminViews/Feedback/List.cshtml", feedbacks);
        }

        public async Task<IActionResult> Detail(Guid id)
        {
            var feedback = await _service.GetDetailAsync(id);

            if (feedback == null)
                return NotFound();

            return View("~/Views/AdminViews/Feedback/Detail.cshtml", feedback);
        }

        public async Task<IActionResult> Toggle(Guid id)
        {
            await _service.ToggleVisibilityAsync(id);

            return RedirectToAction(nameof(AdminList));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);

            return RedirectToAction(nameof(AdminList));
        }

        //Customer

        public async Task<IActionResult> MyFeedback(Guid customerId)
        {
            var feedbacks = await _service.GetByCustomerAsync(customerId);

            return View("~/Views/CustomerViews/Feedback/MyFeedback.cshtml", feedbacks);
        }

        public IActionResult Create()
        {
            return View("~/Views/CustomerViews/Feedback/Create.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateFeedbackDTO dto)
        {
            await _service.CreateAsync(dto);

            return RedirectToAction(nameof(MyFeedback), new { customerId = dto.CustomerId });
        }

        //Staff

        public async Task<IActionResult> Reply(Guid id)
        {
            var feedback = await _service.GetDetailAsync(id);

            return View("~/Views/AdminViews/Feedback/Reply.cshtml", feedback);
        }

        [HttpPost]
        public async Task<IActionResult> Reply(Guid feedbackId, Guid staffId, string reply)
        {
            await _service.ReplyAsync(feedbackId, staffId, reply);

            return RedirectToAction(nameof(AdminList));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteReply(Guid id)
        {
            await _service.DeleteReplyAsync(id);

            return RedirectToAction(nameof(AdminList));
        }
    }
}