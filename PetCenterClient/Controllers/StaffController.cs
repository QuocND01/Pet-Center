using Microsoft.AspNetCore.Mvc;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Controllers
{
    public class StaffController : Controller
    {
        private readonly IStaffService _service;
        public StaffController(IStaffService service) => _service = service;

        public async Task<IActionResult> Index(string searchTerm)
        {
            var staffs = await _service.GetAllAsync();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                staffs = staffs.Where(s => s.FullName.ToLower().Contains(searchTerm) ||
                                         s.Email.ToLower().Contains(searchTerm) ||
                                         s.PhoneNumber.Contains(searchTerm)).ToList();
            }

            ViewData["CurrentFilter"] = searchTerm;
            return View("~/Views/AdminViews/ManageStaff/Index.cshtml", staffs);
        }

        public IActionResult Create() => View("~/Views/AdminViews/ManageStaff/Create.cshtml");

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StaffDto dto)
        {
            // Kiểm tra xem dữ liệu nhập vào có thỏa mãn DataAnnotations không
            if (!ModelState.IsValid)
            {
                return View(dto); // Trả về View cùng với các thông báo lỗi
            }

            var success = await _service.CreateAsync(dto);
            if (success)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "An error occurred while calling the API. Please try again.");
            return View("~/Views/AdminViews/ManageStaff/Create.cshtml", dto);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var staff = await _service.GetByIdAsync(id);
            if (staff == null)
            {
                return NotFound();
            }
            // Đảm bảo staff.BirthDay đã có giá trị từ API trả về
            return View("~/Views/AdminViews/ManageStaff/Edit.cshtml", staff);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, StaffDto dto)
        {
            if (await _service.UpdateAsync(id, dto)) return RedirectToAction(nameof(Index));
            return View("~/Views/AdminViews/ManageStaff/Edit.cshtml", dto);
        }

        // 1. Xem chi tiết nhân viên
        public async Task<IActionResult> Details(Guid id)
        {
            var staff = await _service.GetByIdAsync(id);
            if (staff == null) return NotFound();
            return View("~/Views/AdminViews/ManageStaff/Details.cshtml", staff);
        }

        // 2. Hiện trang xác nhận xóa (GET)
        // Khi bạn bấm nút Delete ở Index, nó sẽ nhảy vào đây để hiện View Delete.cshtml
        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var staff = await _service.GetByIdAsync(id);
            if (staff == null) return NotFound();
            return View("~/Views/AdminViews/ManageStaff/Delete.cshtml", staff);
        }

        // 3. Thực hiện xóa thực tế (POST)
        // Khi bạn bấm "Yes, Delete" ở trang Delete.cshtml, nó mới thực hiện xóa
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}