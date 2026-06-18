using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCenterAPI.DTOs.Requests.Cart;
using PetCenterAPI.Service.Interface;

namespace PetCenterAPI.Controllers
{
    [ApiController]
    [Route("api/cart")]
    [Authorize]
    public class CartsController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartsController(ICartService cartService)
        {
            _cartService = cartService;
        }

        // ============================================================
        // VIEW CART
        // ============================================================
        [HttpGet("{customerId:guid}")]
        public async Task<IActionResult> GetCart(Guid customerId)
        {
            if (!TryGetCustomerId(out var tokenCustomerId))
                return Unauthorized(new { message = "Invalid session." });

            // Always operate on the authenticated customer's own cart.
            var result = await _cartService.GetCartAsync(tokenCustomerId);
            return Ok(result);
        }

        // ============================================================
        // ADD TO CART
        // ============================================================
        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] AddToCartRequestDTO request)
        {
            if (!TryGetCustomerId(out var customerId))
                return Unauthorized(new { message = "Invalid session." });

            if (!ModelState.IsValid)
                return BadRequest(new { message = CollectErrors() });

            var (success, message) = await _cartService.AddToCartAsync(customerId, request);
            return success ? Ok(new { message }) : BadRequest(new { message });
        }

        // ============================================================
        // UPDATE QUANTITY
        // ============================================================
        [HttpPut("details/{cartDetailId:guid}")]
        public async Task<IActionResult> UpdateDetail(Guid cartDetailId, [FromBody] UpdateCartDetailRequestDTO request)
        {
            if (!TryGetCustomerId(out var customerId))
                return Unauthorized(new { message = "Invalid session." });

            if (!ModelState.IsValid)
                return BadRequest(new { message = CollectErrors() });

            var (success, message) = await _cartService.UpdateDetailAsync(customerId, cartDetailId, request);
            return success ? Ok(new { message }) : BadRequest(new { message });
        }

        // ============================================================
        // REMOVE ONE ITEM
        // ============================================================
        [HttpDelete("details/{cartDetailId:guid}")]
        public async Task<IActionResult> DeleteDetail(Guid cartDetailId)
        {
            if (!TryGetCustomerId(out var customerId))
                return Unauthorized(new { message = "Invalid session." });

            var (success, message) = await _cartService.DeleteDetailAsync(customerId, cartDetailId);
            return success ? Ok(new { message }) : BadRequest(new { message });
        }

        // ============================================================
        // CLEAR CART
        // ============================================================
        [HttpDelete("clear/{customerId:guid}")]
        public async Task<IActionResult> Clear(Guid customerId)
        {
            if (!TryGetCustomerId(out var tokenCustomerId))
                return Unauthorized(new { message = "Invalid session." });

            var (success, message) = await _cartService.ClearCartAsync(tokenCustomerId);
            return success ? Ok(new { message }) : BadRequest(new { message });
        }

        // ============================================================
        // HELPERS
        // ============================================================
        private bool TryGetCustomerId(out Guid customerId)
        {
            customerId = Guid.Empty;
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claim, out customerId);
        }

        private string CollectErrors()
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .Where(m => !string.IsNullOrWhiteSpace(m));
            return string.Join(" | ", errors);
        }
    }
}
