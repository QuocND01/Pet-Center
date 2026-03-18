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
        private readonly IImportStockService _importStockService;

        public OrdersController(IOrderServiceClient orderService,
                               IAddressServiceClient addressService,
                               ICustomerService customerService,
                               IOrderDetailServiceClient detailService,
                               IProductService productService,
                               IImportStockService importStockService)
        {
            _orderService = orderService;
            _addressService = addressService;
            _customerService = customerService;
            _detailService = detailService;
            _productService = productService;
            _importStockService = importStockService;
        }

        // 1. ORDER LIST (Includes Search & Filter)
        public async Task<IActionResult> Index(string? search, int? status, DateTime? fromDate, DateTime? toDate)
        {
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");

            var role = HttpContext.Session.GetString("Role");
            var orders = await _orderService.GetAllAsync();

            // Authorization: Customer only sees their own orders
            if (role == "Customer")
            {
                var profile = await _customerService.GetProfileAsync();
                if (profile != null)
                {
                    orders = orders.Where(o => o.CustomerId == profile.CustomerId).ToList();
                }
            }

            // ==========================================
            // START SEARCH & FILTER
            // ==========================================

            // 1. Search by OrderId
            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim().ToLower();
                orders = orders.Where(o => o.OrderId.ToString().ToLower().Contains(search)).ToList();
            }

            // 2. Filter by status
            if (status.HasValue)
            {
                orders = orders.Where(o => o.Status == status.Value).ToList();
            }

            // 3. Filter by from date
            if (fromDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate >= fromDate.Value).ToList();
            }

            // 4. Filter by to date (Add 1 day to include the whole toDate)
            if (toDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate < toDate.Value.AddDays(1)).ToList();
            }

            // Sort newest orders first
            orders = orders.OrderByDescending(o => o.OrderDate).ToList();

            // ==========================================
            // Save filter values to ViewBag to display on the UI Form
            // ==========================================
            ViewBag.Search = search;
            ViewBag.Status = status;
            // HTML5 input type="date" requires yyyy-MM-dd format
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

            return View(orders);
        }

        // 2. ORDER DETAILS (Aggregate data from multiple APIs)
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
                    ProductName = product?.ProductName ?? "Unknown Product",
                    ImageUrl = product?.Images?.FirstOrDefault() ?? "/no-image.png",
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                });
            }

            ViewBag.OrderDetails = fullDetails;
            return View(order);
        }

        // 3. CREATE NEW ORDER (GET)
        public async Task<IActionResult> Create()
        {
            var profile = await _customerService.GetProfileAsync();
            if (profile == null) return RedirectToAction("Login", "Auth");

            var addresses = await _addressService.GetAllAsync();
            ViewBag.AddressList = new SelectList(addresses, "AddressId", "AddressDetails");

            return View(new OrderRequestDTO());
        }

        // 4. CREATE NEW ORDER (POST)
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
                    TempData["Success"] = "Order placed successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }

            var addresses = await _addressService.GetAllAsync();
            ViewBag.AddressList = new SelectList(addresses, "AddressId", "AddressDetails", dto.AddressId);
            return View(dto);
        }

        // 5. EDIT (GET) - Only Admin or Staff allowed
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

        // 6. EDIT (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, OrderRequestDTO dto)
        {
            Console.WriteLine($"--- Bắt đầu Edit Order ID: {id} ---");

            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin" && role != "Staff")
            {
                Console.WriteLine($"[Auth] Từ chối truy cập: Role hiện tại là {role}");
                return RedirectToAction(nameof(Index));
            }

            var order = await _orderService.GetByIdAsync(id);
            if (order == null)
            {
                Console.WriteLine($"[Error] Không tìm thấy Order với ID: {id}");
                return NotFound();
            }

            var details = await _detailService.GetByOrderIdAsync(id);

            var oldStatus = order.Status;
            var newStatus = dto.Status;

            Console.WriteLine($"[Status Check] Old: {oldStatus} | New: {newStatus}");

            if (ModelState.IsValid)
            {
                Console.WriteLine("[Success] ModelState hợp lệ. Bắt đầu xử lý logic kho...");

                // ===== CASE 1: Pending -> Approved =====
                if (oldStatus == 1 && newStatus == 2)
                {
                    Console.WriteLine("-> Đang thực hiện TRỪ KHO (Pending -> Approved)");
                    var itemsToDecrease = new List<DecreaseStockItemDto>(); // Danh sách gom hàng

                    foreach (var item in details)
                    {
                        // 1. Trừ lô ở Inventory (FIFO)
                        var mapping = await _importStockService.DeductFIFO(item.ProductId, item.Quantity);

                        if (string.IsNullOrEmpty(mapping))
                        {
                            // Nếu một món hết hàng, dừng toàn bộ đơn (Rollback lô là chuyện của Inventory API nếu bạn đã code)
                            TempData["Error"] = $"Sản phẩm {item.ProductId} không đủ hàng!";
                            return RedirectToAction(nameof(Index));
                        }

                        // 2. Lưu Mapping vào Detail
                        var updateDto = new OrderDetailRequestDTO
                        {
                            OrderId = id,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice,
                            ImportStockDetailId = mapping
                        };
                        await _detailService.UpdateAsync(item.OrderDetailId, updateDto);

                        // 3. Thêm vào danh sách để trừ tổng ở Product Service sau
                        itemsToDecrease.Add(new DecreaseStockItemDto
                        {
                            ProductId = item.ProductId,
                            Quantity = item.Quantity
                        });
                    }

                    // 4. CẬP NHẬT TỔNG TỒN KHO (GỌI 1 LẦN)
                    try
                    {
                        await _productService.DecreaseStockBulk(itemsToDecrease);
                        Console.WriteLine("[Success] Đã cập nhật tổng tồn kho tại Product Service.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Error] Lỗi cập nhật tổng tồn kho: {ex.Message}");
                        // Lưu ý: Lúc này lô đã trừ, mapping đã lưu, nếu lỗi ở đây sẽ bị lệch data giữa Inven và Product.
                    }
                }

                // ===== CASE 2: Approved -> Cancelled =====
                else if (oldStatus == 2 && newStatus == 0)
                {
                    Console.WriteLine("-> Đang thực hiện HOÀN KHO (Approved -> Cancelled)");
                    var itemsToIncrease = new List<IncreaseStockItemDto>();

                    foreach (var item in details)
                    {
                        if (!string.IsNullOrEmpty(item.ImportStockDetailId))
                        {
                            // 1. Hoàn lô ở Inventory
                            await _importStockService.ReturnStock(item.ImportStockDetailId);

                            // 2. Gom hàng để cộng lại tổng
                            itemsToIncrease.Add(new IncreaseStockItemDto
                            {
                                ProductId = item.ProductId,
                                Quantity = item.Quantity
                            });

                            // 3. Xóa mapping (Tùy chọn)
                            item.ImportStockDetailId = null;
                            // await _detailService.UpdateAsync(...);
                        }
                    }

                    // 4. CỘNG LẠI TỔNG TỒN KHO (GỌI 1 LẦN)
                    if (itemsToIncrease.Any())
                    {
                        await _productService.IncreaseStockBulk(itemsToIncrease);
                        Console.WriteLine("[Success] Đã hoàn tổng tồn kho tại Product Service.");
                    }
                }

                // ===== UPDATE DB =====
                Console.WriteLine("-> Đang gọi UpdateAsync...");
                var success = await _orderService.UpdateAsync(id, dto);

                if (success)
                {
                    TempData["Success"] = "Order updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    Console.WriteLine("[Error] UpdateAsync trả về false.");
                }
            }
            else
            {
                // 🔥 ĐÂY LÀ CHỖ QUAN TRỌNG NHẤT
                Console.WriteLine("[Invalid] ModelState không hợp lệ:");
                foreach (var modelStateKey in ModelState.Keys)
                {
                    var value = ModelState[modelStateKey];
                    foreach (var error in value.Errors)
                    {
                        Console.WriteLine($">> Field: {modelStateKey} | Lỗi: {error.ErrorMessage}");
                    }
                }
            }

            Console.WriteLine("--- Kết thúc hàm Edit (Chưa redirect, trả về View) ---");
            return View(dto);
        }

        // 7. CONFIRM DELETE ORDER (GET) - Shows Delete.cshtml
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
                // Get the details of the items in the order
                var orderDetails = await _detailService.GetByOrderIdAsync(id);

                foreach (var item in orderDetails)
                {
                    // Call ProductService to restore stock quantity
                    var stockRestored = await _productService.IncreaseStockAsync(item.ProductId, item.Quantity);

                    if (!stockRestored)
                    {
                        TempData["Error"] = $"System error: Cannot restore stock for product ID {item.ProductId}.";
                        return RedirectToAction(nameof(Index));
                    }
                }
            }

            // 3. Call Cancel Order API (Update Status = 0)
            var success = await _orderService.DeleteAsync(id);

            if (success)
            {
                if (order.Status == 1)
                    TempData["Success"] = "Pending order canceled successfully!";
                else
                    TempData["Success"] = "Order canceled and stock restored successfully!";
            }
            else
            {
                TempData["Error"] = "Cannot cancel this order. Please check your permissions!";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForwardStatus(Guid id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin" && role != "Staff") return Unauthorized();

            // 1. Get current order information
            var order = await _orderService.GetByIdAsync(id);
            if (order == null || order.Status == 0 || order.Status >= 4)
            {
                TempData["Error"] = "Invalid order for status update.";
                return RedirectToAction(nameof(Index));
            }

            if (order.Status == 1)
            {
                // FIXED: Use the EXISTING _detailService instead of _orderService
                var orderDetails = await _detailService.GetByOrderIdAsync(id);

                foreach (var item in orderDetails)
                {
                    // Call Product/Inventory API to deduct the corresponding quantity
                    var stockUpdated = await _productService.DecreaseStockAsync(item.ProductId, item.Quantity);

                    if (!stockUpdated)
                    {
                        // If stock deduction fails (due to out of stock or Product API error)
                        TempData["Error"] = $"Error: Product ID {item.ProductId} has insufficient stock or API call failed!";
                        return RedirectToAction(nameof(Index));
                    }
                }
            }

            // 3. Increment status (Status + 1)
            var newStatus = order.Status + 1;

            // Package DTO to send for update
            var updateDto = new OrderRequestDTO
            {
                AddressId = order.AddressId,
                Status = newStatus,
                TotalAmount = order.TotalAmount
                // Assign other fields if necessary
            };

            // 4. Call order update API
            var success = await _orderService.UpdateAsync(id, updateDto);

            if (success)
                TempData["Success"] = "Order status updated to the next step successfully!";
            else
                TempData["Error"] = "Error updating order status.";

            return RedirectToAction(nameof(Index));
        }
    }
}