using Microsoft.AspNetCore.Mvc;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ICheckoutService _checkoutService;
        private readonly IProductService _productService;
        private readonly IAddressServiceClient _addressService;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(
            ICheckoutService checkoutService,
            IProductService productService,
            IAddressServiceClient addressService,
            ILogger<CheckoutController> logger)
        {
            _checkoutService = checkoutService;
            _productService = productService;
            _addressService = addressService;
            _logger = logger;
        }

        // GET /Checkout  hoặc  /Checkout/Index
        [HttpGet]
        [Route("Checkout")]
        [Route("Checkout/Index")]
        public async Task<IActionResult> Index([FromQuery] string? items)
        {
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            var customerIdStr = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerIdStr) || !Guid.TryParse(customerIdStr, out var customerId))
                return RedirectToAction("Login", "Auth");

            _logger.LogInformation("[Checkout.Index] customerId={CustomerId} items={Items}",
                customerId, items);

            if (string.IsNullOrEmpty(items))
                return RedirectToAction("Index", "Cart");

            // ── Parse items ───────────────────────────────────────
            var selectedItems = new List<CheckoutCartItemVM>();
            foreach (var part in items.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var seg = part.Trim().Split(':');
                if (seg.Length < 4) { _logger.LogWarning("[Checkout] Skip segment (len<4): {S}", part); continue; }
                if (!Guid.TryParse(seg[0], out var productId)) { _logger.LogWarning("[Checkout] Skip bad productId: {S}", seg[0]); continue; }
                if (!Guid.TryParse(seg[1], out var cartDetailId)) { _logger.LogWarning("[Checkout] Skip bad cartDetailId: {S}", seg[1]); continue; }
                if (!int.TryParse(seg[2], out var qty) || qty < 1) { _logger.LogWarning("[Checkout] Skip bad qty: {S}", seg[2]); continue; }
                if (!decimal.TryParse(seg[3],
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out var price))
                { _logger.LogWarning("[Checkout] Skip bad price: {S}", seg[3]); continue; }

                ReadProductDTO? product = null;
                try { product = await _productService.GetProductByIdAsync(productId); }
                catch (Exception ex) { _logger.LogWarning(ex, "[Checkout] GetProduct failed for {Id}", productId); }

                selectedItems.Add(new CheckoutCartItemVM
                {
                    CartDetailId = cartDetailId,
                    ProductId = productId,
                    Quantity = qty,
                    UnitPrice = price,
                    ProductName = product?.ProductName ?? "Unknown Product",
                    ImageUrl = product?.Images?.FirstOrDefault()
                });
            }

            _logger.LogInformation("[Checkout.Index] Parsed {Count} items, subTotal={Sub}",
                selectedItems.Count, selectedItems.Sum(i => i.SubTotal));

            if (!selectedItems.Any())
                return RedirectToAction("Index", "Cart");

            var subTotal = selectedItems.Sum(i => i.SubTotal);

            // ── Addresses ─────────────────────────────────────────
            var customerAddresses = new List<AddressResponseDTO>();
            try
            {
                var all = await _addressService.GetAllAsync();
                customerAddresses = all.Where(a => a.CustomerId == customerId).ToList();
                _logger.LogInformation("[Checkout.Index] Loaded {Count} addresses", customerAddresses.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Checkout.Index] Failed to load addresses");
            }

            // ── Vouchers ─────────────────────────────────────────
            // KHÔNG dùng try/catch ở đây — để lỗi nổi lên trang 500
            // giúp debug. Sau khi fix xong có thể thêm lại try/catch.
            var vouchers = await _checkoutService.GetAvailableVouchersAsync(customerId, subTotal);
            _logger.LogInformation("[Checkout.Index] Loaded {Count} vouchers for customer {Id}",
                vouchers.Count, customerId);

            var vm = new CheckoutViewModel
            {
                SelectedItems = selectedItems,
                Addresses = customerAddresses,
                AvailableVouchers = vouchers,
                CustomerId = customerId
            };

            return View("~/Views/CustomerViews/Checkout/Index.cshtml", vm);
        }

        // POST /Checkout/PlaceOrder
        [HttpPost]
        [Route("Checkout/PlaceOrder")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderBody? dto)
        {
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
                return Json(new { success = false, message = "Please login to continue." });

            var customerIdStr = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerIdStr) || !Guid.TryParse(customerIdStr, out var customerId))
                return Json(new { success = false, message = "Invalid session. Please login again." });

            if (dto == null)
                return Json(new { success = false, message = "Invalid request." });

            if (dto.Items == null || !dto.Items.Any())
                return Json(new { success = false, message = "No items to checkout." });

            if (dto.AddressId == Guid.Empty)
                return Json(new { success = false, message = "Please select a delivery address." });

            var request = new CheckoutRequestDTO
            {
                CustomerId = customerId,
                AddressId = dto.AddressId,
                AddressSnapshot = dto.AddressSnapshot,
                VoucherId = dto.VoucherId,
                Items = dto.Items.Select(i => new CheckoutItemDTO
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };

            CheckoutResponseDTO? result;
            try { result = await _checkoutService.ProcessCheckoutAsync(request); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Checkout.PlaceOrder] Exception");
                return Json(new { success = false, message = $"Server error: {ex.Message}" });
            }

            if (result == null || !result.Success)
                return Json(new { success = false, message = result?.Message ?? "Checkout failed." });

            return Json(new
            {
                success = true,
                message = result.Message,
                orderId = result.OrderId,
                finalAmount = result.FinalAmount,
                discountAmount = result.DiscountAmount,
                redirectUrl = Url.Action("Success", "Checkout", new { orderId = result.OrderId })
            });
        }

        // GET /Checkout/Success
        [HttpGet]
        [Route("Checkout/Success")]
        public IActionResult Success(Guid orderId)
        {
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            ViewBag.OrderId = orderId;
            return View("~/Views/CustomerViews/Checkout/Success.cshtml");
        }
    }

    public class PlaceOrderBody
    {
        public Guid AddressId { get; set; }
        public string? AddressSnapshot { get; set; }
        public Guid? VoucherId { get; set; }
        public List<OrderLineItem> Items { get; set; } = new();
    }

    public class OrderLineItem
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}