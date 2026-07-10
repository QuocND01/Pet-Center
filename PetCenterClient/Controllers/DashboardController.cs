using Microsoft.AspNetCore.Mvc;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IAnalyticsApiClient _analyticsApi;

        public DashboardController(IAnalyticsApiClient analyticsApi)
        {
            _analyticsApi = analyticsApi;
        }

        public async Task<IActionResult> Index()
        {
            // Kiểm tra quyền
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin" && role != "Vet" && role != "Sale Staff")
            {
                return RedirectToAction("AdminLogin", "Auth");
            }

            // Gọi API lấy dữ liệu và ném sang View
            var metrics = await _analyticsApi.GetDashboardMetricsAsync();
            return View("~/Views/AdminViews/Dashboard/Index.cshtml", metrics);
        }
    }
}