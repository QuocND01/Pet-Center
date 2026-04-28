using CartAPI.DTOs;
using CartAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CartAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartsController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartsController(ICartService cartService)
    {
        _cartService = cartService;
    }

    private static string Timestamp =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"))
        .ToString("dd/MM/yyyy HH:mm");

    /// <summary>
    /// Lấy CustomerId từ JWT claim.
    /// Claim name: "sub" hoặc "nameid" tùy AuthService phát token.
    /// </summary>
    private Guid GetCurrentCustomerId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirstValue("sub");

        if (!Guid.TryParse(raw, out var id))
            throw new UnauthorizedAccessException("Invalid customer identity in token");

        return id;
    }

    // ── GET /api/carts/my ──────────────────────────────────────
    /// <summary>Lấy cart của customer đang đăng nhập</summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMyCart()
    {
        try
        {
            var customerId = GetCurrentCustomerId();
            var cart = await _cartService.GetCartByCustomerIdAsync(customerId);

            if (cart is null)
                return NotFound(new { success = false, message = "Cart not found", data = (object?)null, errors = (object?)null, timestamp = Timestamp });

            return Ok(new { success = true, message = "Success", data = cart, errors = (object?)null, timestamp = Timestamp });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { success = false, message = ex.Message, data = (object?)null, errors = (object?)null, timestamp = Timestamp });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message, data = (object?)null, errors = (object?)null, timestamp = Timestamp });
        }
    }

    // ── GET /api/carts/{cartId} ────────────────────────────────
    /// <summary>Lấy cart theo CartId (kiểm tra ownership)</summary>
    [HttpGet("{cartId:guid}")]
    public async Task<IActionResult> GetById(Guid cartId)
    {
        try
        {
            var customerId = GetCurrentCustomerId();
            var cart = await _cartService.GetCartByIdAsync(cartId);

            if (cart is null)
                return NotFound(new { success = false, message = "Cart not found", data = (object?)null, errors = (object?)null, timestamp = Timestamp });

            // Ownership check: customer chỉ xem được cart của mình
            if (cart.CustomerId != customerId)
                return Forbid();

            return Ok(new { success = true, message = "Success", data = cart, errors = (object?)null, timestamp = Timestamp });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { success = false, message = ex.Message, data = (object?)null, errors = (object?)null, timestamp = Timestamp });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message, data = (object?)null, errors = (object?)null, timestamp = Timestamp });
        }
    }

    // ── POST /api/carts/init ───────────────────────────────────
    /// <summary>
    /// Khởi tạo cart cho customer mới.
    /// Endpoint này dành cho CustomerService gọi nội bộ (service-to-service).
    /// Không yêu cầu JWT customer; dùng API Key hoặc internal policy.
    /// </summary>
    [HttpPost("init")]
    [AllowAnonymous] // CustomerService gọi nội bộ, bảo vệ bằng ApiKey header
    public async Task<IActionResult> InitCart([FromBody] CartInitDto dto,
        [FromHeader(Name = "X-Internal-ApiKey")] string? apiKey)
    {
        // Bảo vệ endpoint nội bộ bằng API Key
        var expectedKey = HttpContext.RequestServices
            .GetRequiredService<IConfiguration>()["InternalApiKey"];

        if (string.IsNullOrEmpty(apiKey) || apiKey != expectedKey)
            return Unauthorized(new { success = false, message = "Invalid internal API key", data = (object?)null, errors = (object?)null, timestamp = Timestamp });

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { success = false, message = "Validation failed", data = (object?)null, errors, timestamp = Timestamp });
        }

        try
        {
            var cart = await _cartService.InitCartAsync(dto);
            return CreatedAtAction(nameof(GetById), new { cartId = cart.CartId },
                new { success = true, message = "Cart initialized", data = cart, errors = (object?)null, timestamp = Timestamp });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message, data = (object?)null, errors = (object?)null, timestamp = Timestamp });
        }
    }

    // ── POST /api/carts/{cartId}/items ─────────────────────────
    /// <summary>Thêm item vào cart (tự động cộng dồn, validate stock)</summary>
    [HttpPost("{cartId:guid}/items")]
    public async Task<IActionResult> AddItem(Guid cartId, [FromBody] CartDetailAddDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { success = false, message = "Validation failed", data = (object?)null, errors, timestamp = Timestamp });
        }

        try
        {
            var customerId = GetCurrentCustomerId();

            // Ownership check
            var cart = await _cartService.GetCartByIdAsync(cartId);
            if (cart is null)
                return NotFound(new { success = false, message = "Cart not found", data = (object?)null, errors = (object?)null, timestamp = Timestamp });

            if (cart.CustomerId != customerId)
                return Forbid();

            var updated = await _cartService.AddItemToCartAsync(cartId, dto);
            return Ok(new { success = true, message = "Item added to cart", data = updated, errors = (object?)null, timestamp = Timestamp });
        }
        catch (ArgumentException ex)
        {
            // Stock không đủ
            return BadRequest(new { success = false, message = ex.Message, data = (object?)null, errors = (object?)null, timestamp = Timestamp });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message, data = (object?)null, errors = (object?)null, timestamp = Timestamp });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { success = false, message = ex.Message, data = (object?)null, errors = (object?)null, timestamp = Timestamp });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message, data = (object?)null, errors = (object?)null, timestamp = Timestamp });
        }
    }

    // ── PUT /api/carts/{cartId}/items/{cartDetailsId} ──────────
    /// <summary>Cập nhật số lượng item (validate stock với số lượng mới)</summary>
    [HttpPut("{cartId:guid}/items/{cartDetailsId:guid}")]
    public async Task<IActionResult> UpdateItem(
        Guid cartId, Guid cartDetailsId, [FromBody] CartDetailUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { success = false, message = "Validation failed", data = (object?)null, errors, timestamp = Timestamp });
        }

        try
        {
            var customerId = GetCurrentCustomerId();

            var cart = await _cartService.GetCartByIdAsync(cartId);
            if (cart is null)
                return NotFound(new { success = false, message = "Cart not found", data = (object?)null, errors = (object?)null, timestamp = Timestamp });

            if (cart.CustomerId != customerId)
                return Forbid();

            var updated = await _cartService.UpdateItemQuantityAsync(cartId, cartDetailsId, dto);
            return Ok(new { success = true, message = "Item updated", data = updated, errors = (object?)null, timestamp = Timestamp });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message, data = (object?)null, errors = (object?)null, timestamp = Timestamp });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message, data = (object?)null, errors = (object?)null, timestamp = Timestamp });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { success = false, message = ex.Message, data = (object?)null, errors = (object?)null, timestamp = Timestamp });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message, data = (object?)null, errors = (object?)null, timestamp = Timestamp });
        }
    }

    // ── DELETE /api/carts/{cartId}/items/{cartDetailsId} ───────
    /// <summary>Xóa một item khỏi cart</summary>
    [HttpDelete("{cartId:guid}/items/{cartDetailsId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid cartId, Guid cartDetailsId)
    {
        try
        {
            var customerId = GetCurrentCustomerId();

            var cart = await _cartService.GetCartByIdAsync(cartId);
            if (cart is null)
                return NotFound(new { success = false, message = "Cart not found", data = (object?)null, errors = (object?)null, timestamp = Timestamp });

            if (cart.CustomerId != customerId)
                return Forbid();

            await _cartService.RemoveItemAsync(cartId, cartDetailsId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message, data = (object?)null, errors = (object?)null, timestamp = Timestamp });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message, data = (object?)null, errors = (object?)null, timestamp = Timestamp });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { success = false, message = ex.Message, data = (object?)null, errors = (object?)null, timestamp = Timestamp });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message, data = (object?)null, errors = (object?)null, timestamp = Timestamp });
        }
    }

    // ── DELETE /api/carts/{cartId}/items ───────────────────────
    /// <summary>Xóa toàn bộ items trong cart (clear - sau khi đặt hàng)</summary>
    [HttpDelete("{cartId:guid}/items")]
    public async Task<IActionResult> ClearCart(Guid cartId)
    {
        try
        {
            var customerId = GetCurrentCustomerId();

            var cart = await _cartService.GetCartByIdAsync(cartId);
            if (cart is null)
                return NotFound(new { success = false, message = "Cart not found", data = (object?)null, errors = (object?)null, timestamp = Timestamp });

            if (cart.CustomerId != customerId)
                return Forbid();

            await _cartService.ClearCartAsync(cartId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message, data = (object?)null, errors = (object?)null, timestamp = Timestamp });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { success = false, message = ex.Message, data = (object?)null, errors = (object?)null, timestamp = Timestamp });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message, data = (object?)null, errors = (object?)null, timestamp = Timestamp });
        }
    }
}