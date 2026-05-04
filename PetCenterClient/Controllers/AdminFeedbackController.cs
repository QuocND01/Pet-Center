using Microsoft.AspNetCore.Mvc;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Controllers
{
    public class AdminFeedbackController : Controller
    {
        private readonly IAdminFeedbackService _feedbackService;

        public AdminFeedbackController(IAdminFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        // ── GET: /AdminFeedback/Index ─────────────────────────
        public IActionResult Index()
        {
            var staffId = HttpContext.Session.GetString("StaffId");
            ViewBag.StaffId = staffId ?? "";
            return View("~/Views/AdminViews/Feedback/Index.cshtml");
        }

        // ── AJAX: GET list với filter + paging ────────────────
        [HttpGet]
        public async Task<IActionResult> GetList(
            int page = 1,
            int pageSize = 10,
            int? rating = null,
            bool? hasReply = null,
            string? keyword = null,
            string? sortBy = null)
        {
            var result = await _feedbackService.GetAllAsync(
                page, pageSize, rating, hasReply, keyword, sortBy);

            if (result == null)
                return Json(new { success = false, message = "Không thể tải dữ liệu." });

            return Json(new { success = true, data = result });
        }

        // ── AJAX: GET detail 1 feedback ───────────────────────
        [HttpGet]
        public async Task<IActionResult> GetDetail(Guid feedbackId)
        {
            var result = await _feedbackService.GetByIdAsync(feedbackId);

            if (result == null)
                return Json(new { success = false, message = "Không tìm thấy feedback." });

            return Json(new { success = true, data = result });
        }

        // ── AJAX: POST reply ──────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Reply([FromBody] ReplyFeedbackDto dto)
        {
            if (dto == null || dto.FeedbackId == Guid.Empty)
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });

            if (string.IsNullOrWhiteSpace(dto.ReplyContent))
                return Json(new { success = false, message = "Nội dung reply không được để trống." });

            // Lấy StaffId từ session nếu dto chưa có
            if (dto.StaffId == Guid.Empty)
            {
                var staffIdStr = HttpContext.Session.GetString("StaffId");
                if (Guid.TryParse(staffIdStr, out var staffId))
                    dto.StaffId = staffId;
            }

            var (success, message) = await _feedbackService.ReplyAsync(dto);
            return Json(new { success, message });
        }

        // ── AJAX: PUT update reply ────────────────────────────
        [HttpPost]
        public async Task<IActionResult> UpdateReply([FromBody] UpdateReplyDto dto)
        {
            if (dto == null || dto.FeedbackId == Guid.Empty)
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });

            if (string.IsNullOrWhiteSpace(dto.ReplyContent))
                return Json(new { success = false, message = "Nội dung reply không được để trống." });

            var (success, message) = await _feedbackService.UpdateReplyAsync(dto);
            return Json(new { success, message });
        }

        // ── AJAX: DELETE reply ────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> DeleteReply([FromBody] Guid feedbackId)
        {
            if (feedbackId == Guid.Empty)
                return Json(new { success = false, message = "FeedbackId không hợp lệ." });

            var (success, message) = await _feedbackService.DeleteReplyAsync(feedbackId);
            return Json(new { success, message });
        }

        // ── AJAX: PATCH toggle visibility ─────────────────────
        [HttpPost]
        public async Task<IActionResult> ToggleVisibility(
            [FromBody] ToggleVisibilityRequest request)
        {
            if (request.FeedbackId == Guid.Empty)
                return Json(new { success = false, message = "FeedbackId không hợp lệ." });

            var (success, message) = await _feedbackService
                .ToggleVisibilityAsync(request.FeedbackId, request.IsVisible);

            return Json(new { success, message });
        }
    }

    // ── Request model nhỏ dùng riêng cho action này ───────────
    public class ToggleVisibilityRequest
    {
        public Guid FeedbackId { get; set; }
        public bool IsVisible { get; set; }
    }
}
