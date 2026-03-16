using Microsoft.AspNetCore.Mvc;
using OrdersAPI.DTOs;
using OrdersAPI.Service.Interface;

namespace OrdersAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckoutController : ControllerBase
    {
        private readonly ICheckoutService _checkoutService;

        public CheckoutController(ICheckoutService checkoutService)
        {
            _checkoutService = checkoutService;
        }

        // POST: api/Checkout
        // Body: CheckoutRequestDTO
        [HttpPost]
        public async Task<IActionResult> ProcessCheckout([FromBody] CheckoutRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.Items == null || !dto.Items.Any())
                return BadRequest(new { success = false, message = "No items provided." });

            if (dto.AddressId == Guid.Empty)
                return BadRequest(new { success = false, message = "Address is required." });

            var result = await _checkoutService.ProcessCheckoutAsync(dto);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(result);
        }

        // GET: api/Checkout/vouchers/{customerId}?orderAmount=500000
        [HttpGet("vouchers/{customerId:guid}")]
        public async Task<IActionResult> GetAvailableVouchers(
            Guid customerId,
            [FromQuery] decimal orderAmount = 0)
        {
            var vouchers = await _checkoutService.GetAvailableVouchersAsync(customerId, orderAmount);
            return Ok(vouchers);
        }

        // POST: api/Checkout/validate-voucher
        [HttpPost("validate-voucher")]
        public async Task<IActionResult> ValidateVoucher([FromBody] VoucherValidateRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _checkoutService.ValidateVoucherAsync(dto);

            if (!result.Valid)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(result);
        }
    }
}