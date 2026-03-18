using Microsoft.AspNetCore.Mvc;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Controllers
{
    public class AdminDashboardController : Controller
    {
        private readonly IStatisticsServiceClient _statsService;

        public AdminDashboardController(IStatisticsServiceClient statsService)
        {
            _statsService = statsService;
        }

        public async Task<IActionResult> Index(int? year)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin" && role != "Staff") return RedirectToAction("Login", "Auth");

            int selectedYear = year ?? DateTime.Now.Year;
            var stats = await _statsService.GetDashboardStatsAsync(selectedYear);

            if (stats == null) stats = new DTOs.DashboardStatsDto();

            // Lưu lại năm đang chọn để View biết
            ViewBag.SelectedYear = selectedYear;
            return View("~/Views/AdminViews/Dashboard/Index.cshtml", stats);
        }
    }
}