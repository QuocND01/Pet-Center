using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Controllers
{
    public class AddressController : Controller
    {
        private readonly IAddressServiceClient _addressService;
        private readonly ICustomerService _customerService;

        // FIX: Đã thêm ICustomerService vào Constructor và gán giá trị
        public AddressController(IAddressServiceClient addressService, ICustomerService customerService)
        {
            _addressService = addressService;
            _customerService = customerService;
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
        public IActionResult Create()
        {
            // Tránh lỗi Null ở View bằng cách khởi tạo model trống
            return View(new AddressCreateDTO());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddressCreateDTO dto)
        {
            var profile = await _customerService.GetProfileAsync();
            if (profile == null) return RedirectToAction("Login", "Auth");

            dto.CustomerId = profile.CustomerId;
            ModelState.Remove("CustomerId"); // Bỏ qua lỗi validation cho ID vì mình đã gán tay

            if (ModelState.IsValid)
            {
                // GỌI API LƯU DỮ LIỆU
                var success = await _addressService.CreateAsync(dto);

                if (success)
                {
                    TempData["Success"] = "Thêm địa chỉ thành công!";
                    return RedirectToAction(nameof(Index));
                }

                // NẾU THẤT BẠI: Hãy kiểm tra logs ở project AddressAPI 
                // hoặc thêm dòng này để hiện lỗi lên giao diện
                ModelState.AddModelError("", "API từ chối lưu. Có thể do trùng dữ liệu hoặc lỗi DB.");
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, AddressCreateDTO dto)
        {
            // Dùng CustomerService để lấy ID từ Session JWT cho đồng bộ
            var profile = await _customerService.GetProfileAsync();
            if (profile == null) return RedirectToAction("Login", "Auth");

            dto.CustomerId = profile.CustomerId; // Lấy ID chuẩn

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

        // 7. XÓA (GET)
        public async Task<IActionResult> Delete(Guid id)
        {
            var address = await _addressService.GetByIdAsync(id);
            if (address == null) return NotFound();
            return View(address);
        }

        // 8. XÓA (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var success = await _addressService.DeleteAsync(id);
            if (success) TempData["Success"] = "Đã xóa địa chỉ.";
            else TempData["Error"] = "Xóa thất bại.";

            return RedirectToAction(nameof(Index));
        }
    }
}