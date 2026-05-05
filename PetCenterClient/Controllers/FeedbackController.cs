using Microsoft.AspNetCore.Mvc;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly IFeedbackService _feedbackService;
        private readonly IProductService _productService;

        public FeedbackController(
            IFeedbackService feedbackService,
            IProductService productService)
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
        public async Task<IActionResult> CreateBulk([FromBody] CreateBulkFeedbackDto dto)
        {
            if (dto?.Feedbacks == null || dto.Feedbacks.Count == 0)
                return Json(new { success = false, message = "Vui lòng nhập đánh giá." });

            var success = await _feedbackService.CreateBulkFeedbackAsync(dto);

            if (success)
                return Json(new { success = true, message = "Đánh giá của bạn đã được gửi thành công!" });

            return Json(new { success = false, message = "Có lỗi xảy ra. Vui lòng thử lại." });
        }

        // POST: Feedback/Update
        // AJAX endpoint để cập nhật feedback
        [HttpPost]
        public async Task<IActionResult> Update([FromBody] UpdateProductFeedbackDto dto)
        {
            if (dto == null || dto.Rating < 1 || dto.Rating > 5)
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });

            var success = await _feedbackService.UpdateFeedbackAsync(dto);

            if (success)
                return Json(new { success = true, message = "Cập nhật đánh giá thành công!" });

            return Json(new { success = false, message = "Có lỗi xảy ra. Vui lòng thử lại." });
        }
    }
}