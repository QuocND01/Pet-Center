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

        public AddressController(IAddressServiceClient addressService, ICustomerService customerService)
        {
            _addressService = addressService;
            _customerService = customerService;
        }

        // 1. DANH SÁCH (READ ALL)
        public async Task<IActionResult> Index()
        {
            var profile = await _customerService.GetProfileAsync();
            if (profile == null) return RedirectToAction("Login", "Auth");
            var allAddresses = await _addressService.GetAllAsync();
            var myAddresses = allAddresses
                .Where(a => a.CustomerId == profile.CustomerId)
                .ToList();

            return View("~/Views/CustomerViews/Address/Index.cshtml", myAddresses);
        }

        // 2. CHI TIẾT (READ ONE)
        public async Task<IActionResult> Details(Guid id)
        {
            var address = await _addressService.GetByIdAsync(id);
            if (address == null) return NotFound();

            return View("~/Views/CustomerViews/Address/Details.cshtml", address);
        }

        // 3. THÊM MỚI (GET)
        public IActionResult Create()
        {
            return View("~/Views/CustomerViews/Address/Create.cshtml", new AddressCreateDTO());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddressCreateDTO dto)
        {
            var profile = await _customerService.GetProfileAsync();
            if (profile == null) return RedirectToAction("Login", "Auth");

            dto.CustomerId = profile.CustomerId;
            ModelState.Remove("CustomerId");

            if (ModelState.IsValid)
            {
                var success = await _addressService.CreateAsync(dto);
                if (success)
                {
                    TempData["Success"] = "Thêm địa chỉ thành công!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "API từ chối lưu. Có thể do trùng dữ liệu hoặc lỗi DB.");
            }
            // Trả về View Create nếu có lỗi
            return View("~/Views/CustomerViews/Address/Create.cshtml", dto);
        }

        // 5. CHỈNH SỬA (GET)
        public async Task<IActionResult> Edit(Guid id)
        {
            var profile = await _customerService.GetProfileAsync();
            if (profile == null) return RedirectToAction("Login", "Auth");

            var address = await _addressService.GetByIdAsync(id);

            // Check xem địa chỉ này có phải của ông đang đăng nhập không
            if (address == null || address.CustomerId != profile.CustomerId)
                return Forbid();

            var editDto = new AddressCreateDTO
            {
                CustomerId = address.CustomerId,
                AddressDetails = address.AddressDetails,
                Province = address.Province,
                District = address.District,
                Ward = address.Ward,
                IsDefault = address.IsDefault
            };
            return View("~/Views/CustomerViews/Address/Edit.cshtml", editDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, AddressCreateDTO dto)
        {
            var profile = await _customerService.GetProfileAsync();
            if (profile == null) return RedirectToAction("Login", "Auth");

            dto.CustomerId = profile.CustomerId;

            if (ModelState.IsValid)
            {
                var success = await _addressService.UpdateAsync(id, dto);
                if (success)
                {
                    TempData["Success"] = "Cập nhật địa chỉ thành công!";
                    return RedirectToAction(nameof(Index));
                }
            }
            return View("~/Views/CustomerViews/Address/Edit.cshtml", dto);
        }

        // 7. XÓA (GET)
        public async Task<IActionResult> Delete(Guid id)
        {
            var address = await _addressService.GetByIdAsync(id);
            if (address == null) return NotFound();

            return View("~/Views/CustomerViews/Address/Delete.cshtml", address);
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