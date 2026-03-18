using Microsoft.AspNetCore.Http;
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
        private readonly ICustomerService _customerService;
        private readonly IOrderDetailServiceClient _detailService;
        private readonly IProductService _productService;

        public OrdersController(IOrderServiceClient orderService,
                               IAddressServiceClient addressService,
                               ICustomerService customerService,
                               IOrderDetailServiceClient detailService,
                               IProductService productService)
        {
            _orderService = orderService;
            _addressService = addressService;
            _customerService = customerService;
            _detailService = detailService;
            _productService = productService;
        }

        // 1. DANH SÁCH ĐƠN HÀNG (Có Search & Filter)
        public async Task<IActionResult> Index(string? search, int? status, DateTime? fromDate, DateTime? toDate)
        {
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");

            var role = HttpContext.Session.GetString("Role");
            var orders = await _orderService.GetAllAsync();

            // Phân quyền: Customer chỉ thấy đơn của mình
            if (role == "Customer")
            {
                var profile = await _customerService.GetProfileAsync();
                if (profile != null)
                {
                    orders = orders.Where(o => o.CustomerId == profile.CustomerId).ToList();
                }
            }

            // ==========================================
            // BẮT ĐẦU TÌM KIẾM & LỌC (Giống StaffController)
            // ==========================================

            // 1. Tìm theo mã đơn hàng (OrderId)
            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim().ToLower();
                orders = orders.Where(o => o.OrderId.ToString().ToLower().Contains(search)).ToList();
            }

            // 2. Lọc theo trạng thái
            if (status.HasValue)
            {
                orders = orders.Where(o => o.Status == status.Value).ToList();
            }

            // 3. Lọc từ ngày
            if (fromDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate >= fromDate.Value).ToList();
            }

            // 4. Lọc đến ngày (Cộng thêm 1 ngày để lấy trọn vẹn ngày toDate)
            if (toDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate < toDate.Value.AddDays(1)).ToList();
            }

            // Sắp xếp đơn hàng mới nhất lên đầu
            orders = orders.OrderByDescending(o => o.OrderDate).ToList();

            // ==========================================
            // Lưu lại giá trị bộ lọc vào ViewBag để hiển thị lại trên Form UI
            // ==========================================
            ViewBag.Search = search;
            ViewBag.Status = status;
            // Input type="date" của HTML5 bắt buộc định dạng yyyy-MM-dd
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

            return View(orders);
        }

        // 2. CHI TIẾT ĐƠN HÀNG (Gom dữ liệu từ nhiều API)
        public async Task<IActionResult> Details(Guid id)
        {
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");

            var order = await _orderService.GetByIdAsync(id);
            if (order == null) return NotFound();

            var rawDetails = await _detailService.GetByOrderIdAsync(id);
            var fullDetails = new List<OrderDetailVM>();

            foreach (var item in rawDetails)
            {
                var product = await _productService.GetProductByIdAsync(item.ProductId);
                fullDetails.Add(new OrderDetailVM
                {
                    ProductId = item.ProductId,
                    ProductName = product?.ProductName ?? "Sản phẩm không xác định",
                    ImageUrl = product?.Images?.FirstOrDefault() ?? "/no-image.png",
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                });
            }

            ViewBag.OrderDetails = fullDetails;
            return View(order);
        }

        // 3. TẠO MỚI ĐƠN HÀNG (GET)
        public async Task<IActionResult> Create()
        {
            var profile = await _customerService.GetProfileAsync();
            if (profile == null) return RedirectToAction("Login", "Auth");

            var addresses = await _addressService.GetAllAsync();
            ViewBag.AddressList = new SelectList(addresses, "AddressId", "AddressDetails");

            return View(new OrderRequestDTO());
        }

        // 4. TẠO MỚI ĐƠN HÀNG (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderRequestDTO dto)
        {
            var profile = await _customerService.GetProfileAsync();
            if (profile == null) return RedirectToAction("Login", "Auth");

            dto.CustomerId = profile.CustomerId;
            dto.OrderDate = DateTime.Now;
            dto.Status = 1;

            ModelState.Remove("CustomerId");

            if (ModelState.IsValid)
            {
                var success = await _orderService.CreateAsync(dto);
                if (success)
                {
                    TempData["Success"] = "Đặt hàng thành công!";
                    return RedirectToAction(nameof(Index));
                }
            }

            var addresses = await _addressService.GetAllAsync();
            ViewBag.AddressList = new SelectList(addresses, "AddressId", "AddressDetails", dto.AddressId);
            return View(dto);
        }

        // 5. CHỈNH SỬA (GET) - Chỉ Admin hoặc Staff mới được vào
        public async Task<IActionResult> Edit(Guid id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin" && role != "Staff") return RedirectToAction(nameof(Index));

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
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin" && role != "Staff") return RedirectToAction(nameof(Index));

            if (ModelState.IsValid)
            {
                var success = await _orderService.UpdateAsync(id, dto);
                if (success)
                {
                    TempData["Success"] = "Cập nhật đơn hàng thành công!";
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(dto);
        }

        // 7. XÁC NHẬN HỦY ĐƠN (GET) - Hiện trang Delete.cshtml
        public async Task<IActionResult> Delete(Guid id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null) return NotFound();
            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null) return NotFound();

            if (order.Status == 2)
            {
                // Lấy chi tiết các món hàng trong đơn ra
                var orderDetails = await _detailService.GetByOrderIdAsync(id);

                foreach (var item in orderDetails)
                {
                    // Gọi ProductService để cộng ngược số lượng kho
                    var stockRestored = await _productService.IncreaseStockAsync(item.ProductId, item.Quantity);

                    if (!stockRestored)
                    {
                        TempData["Error"] = $"Lỗi hệ thống: Không thể hoàn trả số lượng cho sản phẩm mã {item.ProductId}.";
                        return RedirectToAction(nameof(Index));
                    }
                }
            }

            // 3. Tiến hành gọi API Hủy đơn (Cập nhật Status = 0)
            var success = await _orderService.DeleteAsync(id);

            if (success)
            {
                if (order.Status == 1)
                    TempData["Success"] = "Đã hủy đơn hàng chờ xử lý thành công!";
                else
                    TempData["Success"] = "Đã hủy đơn hàng và hoàn trả số lượng vào kho thành công!";
            }
            else
            {
                TempData["Error"] = "Không thể hủy đơn hàng này. Kiểm tra lại quyền hạn!";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForwardStatus(Guid id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin" && role != "Staff") return Unauthorized();

            // 1. Lấy thông tin đơn hàng hiện tại
            var order = await _orderService.GetByIdAsync(id);
            if (order == null || order.Status == 0 || order.Status >= 4)
            {
                TempData["Error"] = "Đơn hàng không hợp lệ để cập nhật trạng thái.";
                return RedirectToAction(nameof(Index));
            }

            if (order.Status == 1)
            {
                // ĐÃ SỬA: Dùng _detailService CÓ SẴN thay vì _orderService
                var orderDetails = await _detailService.GetByOrderIdAsync(id);

                foreach (var item in orderDetails)
                {
                    // Gọi sang API của Product/Inventory để trừ đi số lượng tương ứng
                    var stockUpdated = await _productService.DecreaseStockAsync(item.ProductId, item.Quantity);

                    if (!stockUpdated)
                    {
                        // Nếu trừ kho thất bại (do hết hàng hoặc lỗi API Product)
                        TempData["Error"] = $"Lỗi: Sản phẩm mã {item.ProductId} không đủ số lượng trong kho hoặc API gọi thất bại!";
                        return RedirectToAction(nameof(Index));
                    }
                }
            }

            // 3. Tịnh tiến trạng thái (Status + 1)
            var newStatus = order.Status + 1;

            // Đóng gói DTO để gửi đi update
            var updateDto = new OrderRequestDTO
            {
                AddressId = order.AddressId,
                Status = newStatus,
                TotalAmount = order.TotalAmount
                // Gán thêm các trường khác nếu cần thiết
            };

            // 4. Gọi API update đơn hàng
            var success = await _orderService.UpdateAsync(id, updateDto);

            if (success)
                TempData["Success"] = "Đã cập nhật đơn hàng sang bước tiếp theo thành công!";
            else
                TempData["Error"] = "Lỗi khi cập nhật trạng thái đơn hàng.";

            return RedirectToAction(nameof(Index));
        }


    }
}