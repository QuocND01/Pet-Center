using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PetCenterClient.DTOs; // Đảm bảo đúng namespace chứa DTOs
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Controllers
{
    public class AddressController : Controller
    {
        private readonly IAddressServiceClient _addressService;

        public AddressController(IAddressServiceClient addressService)
        {
            _addressService = addressService;
        }

        // 1. DANH SÁCH (READ ALL)
        public async Task<IActionResult> Index()
        {
            var list = await _addressService.GetAllAsync();
            return View(list);
        }

        // 2. CHI TIẾT (READ ONE)
        public async Task<IActionResult> Details(Guid id)
        {
            var address = await _addressService.GetByIdAsync(id);
            if (address == null) return NotFound();
            return View(address);
        }

        // 3. THÊM MỚI (GET)
        public IActionResult Create() => View();

        // 4. THÊM MỚI (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddressCreateDTO dto)
        {
            // Lấy ID người dùng từ Claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToAction("Login", "Account");

            dto.CustomerId = Guid.Parse(userIdClaim.Value);

            if (ModelState.IsValid)
            {
                var success = await _addressService.CreateAsync(dto);
                if (success)
                {
                    TempData["Success"] = "Thêm địa chỉ mới thành công!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Không thể tạo địa chỉ. Vui lòng thử lại.");
            }
            return View(dto);
        }

        // 5. CHỈNH SỬA (GET)
        public async Task<IActionResult> Edit(Guid id)
        {
            var address = await _addressService.GetByIdAsync(id);
            if (address == null) return NotFound();

            var editDto = new AddressCreateDTO
            {
                CustomerId = address.CustomerId,
                AddressDetails = address.AddressDetails,
                Province = address.Province,
                District = address.District,
                Ward = address.Ward,
                IsDefault = address.IsDefault
            };
            return View(editDto);
        }

        // 6. CHỈNH SỬA (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, AddressCreateDTO dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToAction("Login", "Account");

            // Đảm bảo CustomerId luôn đúng với người đang đăng nhập
            dto.CustomerId = Guid.Parse(userIdClaim.Value);

            if (ModelState.IsValid)
            {
                var success = await _addressService.UpdateAsync(id, dto);
                if (success)
                {
                    TempData["Success"] = "Cập nhật địa chỉ thành công!";
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(dto);
        }

        // 7. XÓA (GET - Xác nhận)
        public async Task<IActionResult> Delete(Guid id)
        {
            var address = await _addressService.GetByIdAsync(id);
            if (address == null) return NotFound();
            return View(address);
        }

        // 8. XÓA (POST - Thực hiện)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var success = await _addressService.DeleteAsync(id);
            if (success)
            {
                TempData["Success"] = "Đã xóa địa chỉ.";
            }
            else
            {
                TempData["Error"] = "Xóa thất bại.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}