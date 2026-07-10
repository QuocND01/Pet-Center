using Microsoft.AspNetCore.Mvc;
using PetCenterClient.Services.Interface;

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
            // Chỉ Admin hoặc Sale mới được xem
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin" && role != "Sale")
            {
                return RedirectToAction("Index", "Home");
            }

            var result = await _orderService.GetOrderListAdminAsync(
                search, status, paymentMethod, sortBy, sortOrder, page);

            int pageSize = 10;
            int totalItems = result?.Count ?? 0;
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

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
            if (role != "Admin" && role != "Sale")
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

        public async Task<IActionResult> History()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Customer")
            {
                return RedirectToAction("Index", "Home");
            }

            var historyList = await _orderService.GetMyOrderHistoryAsync();

            return View("~/Views/CustomerViews/Order/History.cshtml", historyList);
        }

        // Returns only the customer's order list partial for AJAX refresh
        public async Task<IActionResult> HistoryPartial()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Customer") return Forbid();

            var historyList = await _orderService.GetMyOrderHistoryAsync();
            return PartialView("~/Views/CustomerViews/Order/_TableBody.cshtml", historyList);
        }
    }
}