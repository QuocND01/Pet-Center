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

            // 1. Search by Order ID
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
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin" && role != "Staff") return RedirectToAction(nameof(Index));

            if (ModelState.IsValid)
            {
                var success = await _orderService.UpdateAsync(id, dto);
                if (success)
                {
                    TempData["Success"] = "Order updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(dto);
        }

        // 7. CONFIRM DELETE ORDER (GET) - Shows Delete.cshtml
        public async Task<IActionResult> Delete(Guid id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null) return NotFound();
            return View(order);
        }

        // 8. DELETE ORDER (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null) return NotFound();

            // LOGIC RESTORE STOCK: Only restore if order was approved (2) or delivering (3)
            if (order.Status == 2 || order.Status == 3)
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

            // Call Cancel Order API (Update Status = 0)
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

        // 9. FORWARD STATUS (Update Status + Deduct Stock)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForwardStatus(Guid id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin" && role != "Staff") return Unauthorized();

            // Get StaffId of the current user from Session
            var currentUserIdString = HttpContext.Session.GetString("StaffId"); 
            Guid? staffId = null;
            if (Guid.TryParse(currentUserIdString, out Guid parsedId))
            {
                staffId = parsedId;
            }

            // Get current order information
            var order = await _orderService.GetByIdAsync(id);
            if (order == null || order.Status == 0 || order.Status >= 4)
            {
                TempData["Error"] = "Invalid order for status update.";
                return RedirectToAction(nameof(Index));
            }

            // LOGIC DEDUCT STOCK: Only when approving order (Status 1 -> 2)
            if (order.Status == 1)
            {
                var orderDetails = await _detailService.GetByOrderIdAsync(id);

                foreach (var item in orderDetails)
                {
                    // Call Product/Inventory API to deduct the corresponding quantity
                    var stockUpdated = await _productService.DecreaseStockAsync(item.ProductId, item.Quantity);

                    if (!stockUpdated)
                    {
                        // If stock deduction fails
                        TempData["Error"] = $"Error: Product ID {item.ProductId} has insufficient stock or API call failed!";
                        return RedirectToAction(nameof(Index));
                    }
                }
            }

            // Increment status (Status + 1)
            var newStatus = order.Status + 1;

            // Package DTO to send for update
            var updateDto = new OrderRequestDTO
            {
                CustomerId = order.CustomerId,
                AddressId = order.AddressId,
                AddressSnapshot = order.AddressSnapshot,
                TotalAmount = order.TotalAmount,
                DiscountAmount = order.DiscountAmount,
                Status = newStatus,
                StaffId = staffId 
            };

            if (newStatus == 4)
            {
                updateDto.DeliveredDate = DateTime.Now;
            }

            // Call order update API
            var success = await _orderService.UpdateAsync(id, updateDto);

            if (success)
            {
                if (newStatus == 4)
                    TempData["Success"] = "Order completed successfully! Delivered date and Staff ID have been recorded.";
                else
                    TempData["Success"] = "Order status updated to the next step successfully!";
            }
            else
            {
                TempData["Error"] = "Error updating order status.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}