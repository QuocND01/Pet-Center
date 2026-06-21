using Microsoft.AspNetCore.Mvc;
using PetCenterClient.DTOs;
using PetCenterClient.Services;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly IFeedbackApiService _feedbackService;
        private readonly IProductAPIClient _productService;

        public FeedbackController(
            IFeedbackApiService feedbackService,
            IProductAPIClient productService)
        {
            _feedbackService = feedbackService;
            _productService = productService;
        }

        // ============================================================
        // FEEDBACK — CHECK ORDER FEEDBACK STATUS
        // ============================================================

        // GET: Feedback/CheckOrderFeedback?orderId=xxx
        // AJAX endpoint để kiểm tra order đã feedback chưa
        [HttpGet]
        public async Task<IActionResult> CheckOrderFeedback(Guid orderId)
        {
            var hasFeedback = await _feedbackService.HasFeedbackForOrderAsync(orderId);
            return Json(new { success = true, hasFeedback });
        }

        // GET: Feedback/GetOrderFeedbacks?orderId=xxx
        // AJAX endpoint để lấy danh sách feedback của order
        [HttpGet]
        public async Task<IActionResult> GetOrderFeedbacks(Guid orderId)
        {
            var feedbacks = await _feedbackService.GetFeedbacksByOrderIdAsync(orderId);
            return Json(new { success = true, data = feedbacks });
        }

        // ============================================================
        // FEEDBACK — CREATE BULK (CUSTOMER SIDE)
        // ============================================================

        // POST: Feedback/CreateBulk
        // AJAX endpoint để submit feedback nhiều sản phẩm
        [HttpPost]
        public async Task<IActionResult> CreateBulk()
        {
            try
            {
                var form = Request.Form;
                if (!form.ContainsKey("Feedbacks[0].ProductId"))
                    return Json(new { success = false, message = "Please enter at least one review." });

                // Rebuild the multipart content to forward to backend
                var forwardContent = new MultipartFormDataContent();

                foreach (var key in form.Keys)
                    foreach (var val in form[key])
                        forwardContent.Add(new StringContent(val ?? ""), key);

                foreach (var file in form.Files)
                {
                    var stream = file.OpenReadStream();
                    var fileContent = new StreamContent(stream);
                    fileContent.Headers.ContentType =
                        new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                    forwardContent.Add(fileContent, file.Name, file.FileName);
                }

                var (success, message) = await _feedbackService.CreateBulkFeedbackAsync(forwardContent);

                return Json(new { success, message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ============================================================
        // FEEDBACK — UPDATE (CUSTOMER SIDE)
        // ============================================================

        // POST: Feedback/Update
        // AJAX endpoint để cập nhật feedback
        [HttpPost]
        public async Task<IActionResult> Update()
        {
            try
            {
                var form = Request.Form;
                if (!form.ContainsKey("FeedbackId"))
                    return Json(new { success = false, message = "Invalid request data." });

                var forwardContent = new MultipartFormDataContent();

                foreach (var key in form.Keys)
                    foreach (var val in form[key])
                        forwardContent.Add(new StringContent(val ?? ""), key);

                foreach (var file in form.Files)
                {
                    var stream = file.OpenReadStream();
                    var fileContent = new StreamContent(stream);
                    fileContent.Headers.ContentType =
                        new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                    forwardContent.Add(fileContent, file.Name, file.FileName);
                }

                var (success, message) = await _feedbackService.UpdateFeedbackAsync(forwardContent);
                return Json(new { success, message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}