using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IOrderServiceClient _orderService;
        private readonly IAddressServiceClient _addressService;

        public OrdersController(IOrderServiceClient orderService, IAddressServiceClient addressService)
        {
            _orderService = orderService;
            _addressService = addressService;
        }

        // 1. DANH SÁCH ĐƠN HÀNG (Index)
        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetAllAsync();
            return View(orders);
        }

        // 2. CHI TIẾT ĐƠN HÀNG (Details)
        public async Task<IActionResult> Details(Guid id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null) return NotFound();
            return View(order);
        }

        // 3. TẠO MỚI (GET)
        public async Task<IActionResult> Create()
        {
            // Lấy danh sách địa chỉ của người dùng để chọn trong Dropdown
            var addresses = await _addressService.GetAllAsync();
            ViewBag.AddressList = new SelectList(addresses, "AddressId", "AddressDetails");
            return View();
        }

        // 4. TẠO MỚI (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderRequestDTO dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToAction("Login", "Account");

            dto.CustomerId = Guid.Parse(userIdClaim.Value); // Tự động gán ID người dùng
            dto.OrderDate = DateTime.Now;
            dto.Status = 1; // Giả sử 1 là trạng thái "Chờ xử lý"

            if (ModelState.IsValid)
            {
                var success = await _orderService.CreateAsync(dto);
                if (success)
                {
                    TempData["Success"] = "Đặt hàng thành công!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Lỗi hệ thống khi tạo đơn hàng.");
            }

            var addresses = await _addressService.GetAllAsync();
            ViewBag.AddressList = new SelectList(addresses, "AddressId", "AddressDetails");
            return View(dto);
        }

        // 5. CHỈNH SỬA (GET)
        public async Task<IActionResult> Edit(Guid id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null) return NotFound();

            var addresses = await _addressService.GetAllAsync();
            ViewBag.AddressList = new SelectList(addresses, "AddressId", "AddressDetails", order.AddressId);

            var editDto = new OrderRequestDTO
            {
                CustomerId = order.CustomerId,
                StaffId = order.StaffId,
                AddressId = order.AddressId,
                AddressSnapshot = order.AddressSnapshot,
                TotalAmount = order.TotalAmount,
                DiscountAmount = order.DiscountAmount,
                Status = order.Status
            };

            return View(editDto);
        }

        // 6. CHỈNH SỬA (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, OrderRequestDTO dto)
        {
            if (ModelState.IsValid)
            {
                var success = await _orderService.UpdateAsync(id, dto);
                if (success)
                {
                    TempData["Success"] = "Cập nhật đơn hàng thành công!";
                    return RedirectToAction(nameof(Index));
                }
            }

            var addresses = await _addressService.GetAllAsync();
            ViewBag.AddressList = new SelectList(addresses, "AddressId", "AddressDetails", dto.AddressId);
            return View(dto);
        }

        // 7. XÓA (GET - Xác nhận)
        public async Task<IActionResult> Delete(Guid id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null) return NotFound();
            return View(order);
        }

        // 8. XÓA (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var success = await _orderService.DeleteAsync(id);
            if (success)
            {
                TempData["Success"] = "Đã hủy đơn hàng thành công!";
            }
            else
            {
                TempData["Error"] = "Không thể hủy đơn hàng này.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}