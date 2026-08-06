using Microsoft.AspNetCore.Mvc;
using PetCenterClient.Services.Interface;
using PetCenterClient.ViewModels;

namespace PetCenterClient.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IOrderAPIClient _orderService;

        public OrdersController(IOrderAPIClient orderService)
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> IndexAdminAsync(
            string? search,
            int? status,
            string? paymentMethod,
            string? sortBy,
            string sortOrder = "desc",
            int page = 1)
        {
            //// Chỉ Admin hoặc Sale mới được xem
            //var role = HttpContext.Session.GetString("Role");
            //if (role != "Admin" && role != "Sale Staff")
            //{
            //    return RedirectToAction("Index", "Home");
            //}

            var result = await _orderService.GetOrderListAdminAsync(
                search, status, paymentMethod, sortBy, sortOrder, page);

            int pageSize = 10;
            int totalItems = result?.Count ?? 0;
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Expose total item count to the view so the header can display the actual
            // number of orders instead of a pages*pageSize approximation.
            ViewBag.TotalItems = totalItems;

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.PaymentMethod = paymentMethod;
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;

            return View("~/Views/AdminViews/Order/Index.cshtml", result?.Values);
        }

        // Returns only the table body partial so clients can refresh the list via AJAX
        public async Task<IActionResult> IndexAdminPartial(string? search, int? status, string? paymentMethod, string? sortBy, string sortOrder = "desc", int page = 1)
        {
            var role = HttpContext.Session.GetString("Role");
            // Role stored in session uses the string "Sale Staff" (see _AdminLayout),
            // ensure the partial endpoint allows the same role so sales staff can
            // receive AJAX partial refreshes without needing a full page reload.
            if (role != "Admin" && role != "Sale Staff")
            {
                return Forbid();
            }

            var result = await _orderService.GetOrderListAdminAsync(search, status, paymentMethod, sortBy, sortOrder, page);
            return PartialView("~/Views/AdminViews/Order/_TableBody.cshtml", result?.Values);
        }

        // GET: Orders/Details/{id}
        public async Task<IActionResult> DetailsAsync(Guid id)
        {
            var orderDetail = await _orderService.GetOrderDetailsAsync(id);
            if (orderDetail == null)
            {
                return NotFound();
            }
            // Trả về một PartialView để nhúng vào Modal Bootstrap có sẵn ở trang danh sách
            return PartialView("~/Views/AdminViews/Order/_Details.cshtml", orderDetail);
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var success = await _orderService.CancelOrderAsync(id);
            if (success)
            {
                return Json(new { success = true, message = "Order has been cancelled successfully." });
            }
            return Json(new { success = false, message = "Failed to cancel the order." });
        }

        [HttpPost]
        public async Task<IActionResult> AdvanceStatus(Guid id)
        {
            var success = await _orderService.AdvanceOrderStatusAsync(id);
            if (success)
            {
                return Json(new { success = true, message = "Order status has been advanced successfully." });
            }
            return Json(new { success = false, message = "Failed to update order status." });
        }

        public async Task<IActionResult> History(string? search, int? status, string? paymentMethod)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Customer")
            {
                return RedirectToAction("Index", "Home");
            }

            var historyList = await _orderService.GetMyOrderHistoryAsync();

            var list = historyList == null ? new List<ReadOrderListViewModel>() : historyList;

            // Enrich each order with small preview items so we can display thumbnails and search by product name
            foreach (var o in list)
            {
                try
                {
                    var detail = await _orderService.GetOrderDetailsAsync(o.OrderId);
                    if (detail?.OrderItems != null)
                    {
                        o.OrderItemsPreview = detail.OrderItems.Take(3).ToList();
                    }
                }
                catch
                {
                    // ignore failures per-order
                }
            }

            // Apply filters (search includes product name now)
            if (!string.IsNullOrEmpty(search))
            {
                var q = search.Trim();
                if (q.StartsWith("#")) q = q.TrimStart('#').Trim();
                list = list.Where(o => o.OrderId.ToString().IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                                       || (o.CustomerName ?? string.Empty).IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                                       || (o.OrderItemsPreview != null && o.OrderItemsPreview.Any(it => it.ProductName.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)))
                           .ToList();
            }

            if (status.HasValue)
            {
                list = list.Where(o => o.Status == status.Value).ToList();
            }

            if (!string.IsNullOrEmpty(paymentMethod))
            {
                list = list.Where(o => string.Equals(o.PaymentMethod, paymentMethod, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.PaymentMethod = paymentMethod;

            // Header statistics
            ViewBag.TotalOrders = list.Count;
            ViewBag.TotalSpent = list.Sum(o => o.TotalAmount);

            return View("~/Views/CustomerViews/Order/History.cshtml", list);
        }

        // Returns only the customer's order list partial for AJAX refresh
        public async Task<IActionResult> HistoryPartial(string? search, int? status, string? paymentMethod)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Customer") return Forbid();

            var historyList = await _orderService.GetMyOrderHistoryAsync();

            // Enrich each order with a small preview of its items (images + names)
            var list = historyList == null ? new List<ReadOrderListViewModel>() : historyList;
            foreach (var o in list)
            {
                try
                {
                    var detail = await _orderService.GetOrderDetailsAsync(o.OrderId);
                    if (detail?.OrderItems != null)
                    {
                        o.OrderItemsPreview = detail.OrderItems.Take(3).ToList();
                    }
                }
                catch
                {
                    // ignore per-order detail failures
                }
            }

            // Apply filters (search includes product name now)
            if (!string.IsNullOrEmpty(search))
            {
                var q = search.Trim();
                if (q.StartsWith("#")) q = q.TrimStart('#').Trim();
                list = list.Where(o => o.OrderId.ToString().IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                                         || (o.CustomerName ?? string.Empty).IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                                         || (o.OrderItemsPreview != null && o.OrderItemsPreview.Any(it => it.ProductName.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)))
                               .ToList();
            }

            if (status.HasValue)
            {
                list = list.Where(o => o.Status == status.Value).ToList();
            }

            if (!string.IsNullOrEmpty(paymentMethod))
            {
                list = list.Where(o => string.Equals(o.PaymentMethod, paymentMethod, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return PartialView("~/Views/CustomerViews/Order/_TableBody.cshtml", list);
        }
    }
}