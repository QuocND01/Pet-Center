using Microsoft.AspNetCore.Mvc;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IAnalyticsApiClient _analyticsApi;

        // Tiêm Service thông qua Dependency Injection xịn xò
        public DashboardController(IAnalyticsApiClient analyticsApi)
        {
            _analyticsApi = analyticsApi;
        }

        // 1. Luồng load giao diện ban đầu (Server-Side Rendering dữ liệu mặc định)
        public async Task<IActionResult> Index()
        {
            // Kiểm tra quyền hạn phân hệ Admin
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin" && role != "Vet" && role != "Sale Staff")
            {
                return RedirectToAction("Login", "Auth");
            }

            // Gọi service lấy data mặc định (đầu tháng đến nay) để nạp sẵn vào View
            var initialMetrics = await _analyticsApi.GetDashboardMetricsAsync();

            return View("~/Views/AdminViews/Dashboard/Index.cshtml", initialMetrics);
        }

        // 2. Luồng xử lý Lọc dữ liệu bằng AJAX (Đóng vai trò Proxy điều hướng)
        [HttpGet]
        public async Task<IActionResult> GetMetricsJson(string startDate, string endDate)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin" && role != "Vet" && role != "Sale Staff")
            {
                return Unauthorized();
            }

            // Gọi sang Backend API thông qua lớp Service đã bọc sẵn Token
            var metrics = await _analyticsApi.GetDashboardMetricsAsync(startDate, endDate);

            // Trả về dữ liệu JSON cho Chart.js vẽ lại biểu đồ
            return Json(metrics);
        }
    }
}