using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;
using PetCenterClient.ViewModels.ManageStaff;

namespace PetCenterClient.Controllers
{
    public class StaffController : Controller
    {
        private const int PageSize = 10;
        private readonly IStaffService _service;

        public StaffController(IStaffService service) => _service = service;

        private bool IsAdmin() =>
            !string.IsNullOrEmpty(HttpContext.Session.GetString("JWT")) &&
            HttpContext.Session.GetString("Role") == "Admin";

        // ============================================================
        // VIEW LIST (search + filter by role/status + pagination)
        // ============================================================
        public async Task<IActionResult> Index(
            string? search,
            string status = "active",
            Guid? roleId = null,
            int page = 1)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Auth");

            var all = await _service.GetAllAsync();
            var roles = await _service.GetRolesAsync();

            // Status filter (default: active only)
            all = status switch
            {
                "inactive" => all.Where(s => !s.IsActive).ToList(),
                "all" => all,
                _ => all.Where(s => s.IsActive).ToList()
            };

            // Role filter
            if (roleId.HasValue && roleId.Value != Guid.Empty)
                all = all.Where(s => s.RoleId == roleId.Value).ToList();

            // Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = RemoveDiacritics(search.Trim().ToLower());
                all = all.Where(s =>
                    RemoveDiacritics((s.FullName ?? "").ToLower()).Contains(q) ||
                    (s.Email ?? "").ToLower().Contains(search.Trim().ToLower()) ||
                    (s.PhoneNumber ?? "").Contains(search.Trim())
                ).ToList();
            }

            all = all.OrderBy(s => s.FullName).ToList();

            var total = all.Count;
            var totalPages = (int)Math.Ceiling(total / (double)PageSize);
            if (page < 1) page = 1;
            if (totalPages > 0 && page > totalPages) page = totalPages;

            var items = all.Skip((page - 1) * PageSize).Take(PageSize).ToList();

            var vm = new StaffIndexViewModel
            {
                Items = items,
                Roles = roles,
                TotalCount = total,
                CurrentPage = page,
                TotalPages = Math.Max(totalPages, 1),
                PageSize = PageSize,
                Search = search,
                Status = status,
                RoleId = roleId
            };

            return View("~/Views/AdminViews/ManageStaff/Index.cshtml", vm);
        }

        // ============================================================
        // VIEW DETAIL (page)
        // ============================================================
        public async Task<IActionResult> Details(Guid id)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Auth");

            var staff = await _service.GetByIdAsync(id);
            if (staff == null) return NotFound();

            return View("~/Views/AdminViews/ManageStaff/Details.cshtml", staff);
        }

        // ============================================================
        // VIEW DETAIL (JSON — used to prefill the Edit modal)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> GetDetail(Guid id)
        {
            if (!IsAdmin()) return Json(new { success = false, message = "Unauthorized" });

            var staff = await _service.GetByIdAsync(id);
            if (staff == null) return Json(new { success = false, message = "Staff not found" });

            return Json(new { success = true, data = staff });
        }

        // ============================================================
        // CREATE (modal -> POST)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateStaffDto dto)
        {
            if (!IsAdmin()) return Json(new { success = false, message = "Unauthorized" });

            if (!ModelState.IsValid)
                return Json(new { success = false, message = CollectModelErrors() });

            var (success, message) = await _service.CreateAsync(dto);
            if (success) TempData["StaffSuccess"] = message;
            return Json(new { success, message });
        }

        // ============================================================
        // UPDATE (modal -> POST)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UpdateStaffDto dto)
        {
            if (!IsAdmin()) return Json(new { success = false, message = "Unauthorized" });

            if (!ModelState.IsValid)
                return Json(new { success = false, message = CollectModelErrors() });

            var (success, message) = await _service.UpdateAsync(id, dto);
            if (success) TempData["StaffSuccess"] = message;
            return Json(new { success, message });
        }

        // ============================================================
        // DELETE (confirm modal -> POST, soft delete)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Auth");

            var (success, message) = await _service.DeleteAsync(id);
            TempData[success ? "StaffSuccess" : "StaffError"] = message;
            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // HELPERS
        // ============================================================
        private string CollectModelErrors()
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .Where(m => !string.IsNullOrWhiteSpace(m));
            return string.Join(" | ", errors);
        }

        private static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
