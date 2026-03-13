using Microsoft.AspNetCore.Mvc;
using OrdersAPI.DTOs;
using OrdersAPI.Service.Interface;

namespace OrdersAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        // GET: api/Cart/{customerId}
        [HttpGet("{customerId:guid}")]
        public async Task<IActionResult> GetCart(Guid customerId)
        {
            var cart = await _cartService.GetCartByCustomerIdAsync(customerId);
            if (cart == null)
                return Ok(new CartResponseDTO { CustomerId = customerId, CartDetails = new() });

            return Ok(cart);
        }

        // POST: api/Cart/add
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, message) = await _cartService.AddToCartAsync(dto);

            if (!success)
                return BadRequest(new { success = false, message });

            return Ok(new { success = true, message });
        }

        // PUT: api/Cart/details/{cartDetailId}
        [HttpPut("details/{cartDetailId:guid}")]
        public async Task<IActionResult> UpdateCartDetail(Guid cartDetailId, [FromBody] UpdateCartDetailRequestDTO dto)
        {
            var (success, message) = await _cartService.UpdateCartDetailAsync(cartDetailId, dto.Quantity);

            if (!success)
                return BadRequest(new { success = false, message });

            return Ok(new { success = true, message });
        }

        // DELETE: api/Cart/details/{cartDetailId}
        [HttpDelete("details/{cartDetailId:guid}")]
        public async Task<IActionResult> DeleteCartDetail(Guid cartDetailId)
        {
            var (success, message) = await _cartService.DeleteCartDetailAsync(cartDetailId);

            if (!success)
                return NotFound(new { success = false, message });

            return Ok(new { success = true, message });
        }

        // DELETE: api/Cart/clear/{customerId}
        [HttpDelete("clear/{customerId:guid}")]
        public async Task<IActionResult> ClearCart(Guid customerId)
        {
            var (success, message) = await _cartService.ClearCartAsync(customerId);

            if (!success)
                return NotFound(new { success = false, message });

            return Ok(new { success = true, message });
        }
    }
}