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

            if (string.IsNullOrEmpty(items))
                return RedirectToAction("Index", "Cart");

            // ── Parse items ──────────────────────────────────────────────
            var selectedItems = new List<CheckoutCartItemVM>();
            foreach (var part in items.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var seg = part.Trim().Split(':');
                if (seg.Length < 4) continue;
                if (!Guid.TryParse(seg[0], out var productId)) continue;
                if (!Guid.TryParse(seg[1], out var cartDetailId)) continue;
                if (!int.TryParse(seg[2], out var qty) || qty < 1) continue;
                if (!decimal.TryParse(seg[3],
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out var price)) continue;

                ReadProductDTO? product = null;
                try { product = await _productService.GetProductByIdAsync(productId); }
                catch { }

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

            if (!selectedItems.Any())
                return RedirectToAction("Index", "Cart");

            var subTotal = selectedItems.Sum(i => i.SubTotal);

            // ── Lấy địa chỉ active của customer (dùng endpoint mới) ──────
            var customerAddresses = new List<AddressResponseDTO>();
            try
            {
                // Gọi GET /address-service/Addresses/customer/{customerId}
                // → chỉ trả về address IsActive = true, default lên đầu
                customerAddresses = await _addressService.GetByCustomerIdAsync(customerId);
                _logger.LogInformation("[Checkout] Loaded {Count} addresses for customer {Id}",
                    customerAddresses.Count, customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Checkout] Failed to load addresses");
            }

            // ── Vouchers ─────────────────────────────────────────────────
            var vouchers = new List<CustomerVoucherDTO>();
            try
            {
                vouchers = await _checkoutService.GetAvailableVouchersAsync(customerId, subTotal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Checkout] Failed to load vouchers");
            }

            var vm = new CheckoutViewModel
            {
                SelectedItems = selectedItems,
                Addresses = customerAddresses,
                AvailableVouchers = vouchers,
                CustomerId = customerId
            };

            return View("~/Views/CustomerViews/Checkout/Index.cshtml", vm);
        }

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