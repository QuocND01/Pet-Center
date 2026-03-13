using Microsoft.AspNetCore.Mvc;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IProductService _productService;

        public CartController(
            ICartService cartService,
            IProductService productService)
        {
            _cartService = cartService;
            _productService = productService;
        }

        // GET: /Cart
        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            var customerIdStr = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerIdStr) || !Guid.TryParse(customerIdStr, out var customerId))
                return RedirectToAction("Login", "Auth");

            var cart = await _cartService.GetCartAsync(customerId);

            // Enrich cart details with product information
            var enrichedDetails = new List<CartDetailResponseDTO>();
            if (cart?.CartDetails != null)
            {
                foreach (var detail in cart.CartDetails)
                {
                    var product = await _productService.GetProductByIdAsync(detail.ProductId);
                    enrichedDetails.Add(new CartDetailResponseDTO
                    {
                        CartDetailId = detail.CartDetailId,
                        CartId = detail.CartId,
                        ProductId = detail.ProductId,
                        Quantity = detail.Quantity,
                        ProductName = product?.ProductName ?? "Unknown Product",
                        ProductPrice = product?.ProductPrice,
                        ImageUrl = product?.Images?.FirstOrDefault() ?? "/images/no-image.png",
                        StockQuantity = product?.StockQuantity
                    });
                }
            }

            var viewModel = new CartResponseDTO
            {
                CartId = cart?.CartId ?? Guid.Empty,
                CustomerId = customerId,
                CartDetails = enrichedDetails
            };

            return View("~/Views/CustomerViews/Cart/Index.cshtml", viewModel);
        }

        // POST: /Cart/AddToCart  (called from Product Details page via AJAX)
        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequestDTO dto)
        {
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
                return Json(new { success = false, message = "Please login to add items to cart." });

            var customerIdStr = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerIdStr) || !Guid.TryParse(customerIdStr, out var customerId))
                return Json(new { success = false, message = "Please login to add items to cart." });

            dto.CustomerId = customerId;

            if (dto.Quantity <= 0)
                return Json(new { success = false, message = "Quantity must be at least 1." });

            var (success, message) = await _cartService.AddToCartAsync(dto);
            return Json(new { success, message });
        }

        // PUT: /Cart/UpdateDetail  (AJAX)
        [HttpPost]
        public async Task<IActionResult> UpdateDetail([FromBody] UpdateDetailRequest request)
        {
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
                return Json(new { success = false, message = "Unauthorized." });

            if (request.Quantity <= 0)
                return Json(new { success = false, message = "Quantity must be at least 1." });

            var (success, message) = await _cartService.UpdateCartDetailAsync(request.CartDetailId, request.Quantity);
            return Json(new { success, message });
        }

        // POST: /Cart/DeleteDetail  (AJAX)
        [HttpPost]
        public async Task<IActionResult> DeleteDetail([FromBody] DeleteDetailRequest request)
        {
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
                return Json(new { success = false, message = "Unauthorized." });

            var (success, message) = await _cartService.DeleteCartDetailAsync(request.CartDetailId);
            return Json(new { success, message });
        }

        // POST: /Cart/ClearCart  (AJAX)
        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
                return Json(new { success = false, message = "Unauthorized." });

            var customerIdStr = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerIdStr) || !Guid.TryParse(customerIdStr, out var customerId))
                return Json(new { success = false, message = "Unauthorized." });

            var (success, message) = await _cartService.ClearCartAsync(customerId);
            return Json(new { success, message });
        }
    }

    // Helper request DTOs used by controller actions
    public class UpdateDetailRequest
    {
        public Guid CartDetailId { get; set; }
        public int Quantity { get; set; }
    }

    public class DeleteDetailRequest
    {
        public Guid CartDetailId { get; set; }
    }
}