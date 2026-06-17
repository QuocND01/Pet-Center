using Microsoft.AspNetCore.Mvc;
using PetCenterClient.DTOs;
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

        // POST: Feedback/CreateBulk
        // AJAX endpoint để submit feedback nhiều sản phẩm
        [HttpPost]
        public async Task<IActionResult> CreateBulk()
        {
            try
            {
                var form = Request.Form;

                if (!form.ContainsKey("Feedbacks[0].ProductId"))
                    return Json(new { success = false, message = "Vui lòng nhập đánh giá." });

                // Build lại MultipartFormDataContent để forward sang FeedbackAPI
                var forwardContent = new MultipartFormDataContent();

                // Forward tất cả text fields (ProductId, OrderId, Rating, Comment)
                foreach (var key in form.Keys)
                {
                    foreach (var val in form[key])
                    {
                        forwardContent.Add(new StringContent(val ?? ""), key);
                    }
                }

                // Forward tất cả files
                foreach (var file in form.Files)
                {
                    var stream = file.OpenReadStream();
                    var fileContent = new StreamContent(stream);
                    fileContent.Headers.ContentType =
                        new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                    forwardContent.Add(fileContent, file.Name, file.FileName);
                }

                var success = await _feedbackService.CreateBulkFeedbackAsync(forwardContent);

                if (success)
                    return Json(new
                    {
                        success = true,
                        message = "Đánh giá của bạn đã được gửi thành công!"
                    });

                return Json(new { success = false, message = "Có lỗi xảy ra. Vui lòng thử lại." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Feedback/Update
        // AJAX endpoint để cập nhật feedback
        [HttpPost]
        public async Task<IActionResult> Update()
        {
            try
            {
                var form = Request.Form;

                if (!form.ContainsKey("FeedbackId"))
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ." });

                // Build lại MultipartFormDataContent để forward sang FeedbackAPI
                var forwardContent = new MultipartFormDataContent();

                // Forward text fields (FeedbackId, Rating, Comment, RemovedPublicIds)
                foreach (var key in form.Keys)
                {
                    foreach (var val in form[key])
                    {
                        forwardContent.Add(new StringContent(val ?? ""), key);
                    }
                }

                // Forward file mới nếu có
                foreach (var file in form.Files)
                {
                    var stream = file.OpenReadStream();
                    var fileContent = new StreamContent(stream);
                    fileContent.Headers.ContentType =
                        new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                    forwardContent.Add(fileContent, file.Name, file.FileName);
                }

                var success = await _feedbackService.UpdateFeedbackAsync(forwardContent);

                if (success)
                    return Json(new
                    {
                        success = true,
                        message = "Cập nhật đánh giá thành công!"
                    });

                return Json(new { success = false, message = "Có lỗi xảy ra. Vui lòng thử lại." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}