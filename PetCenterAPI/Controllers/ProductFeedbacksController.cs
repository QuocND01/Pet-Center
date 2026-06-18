using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PetCenterAPI.Service.Interface;

namespace PetCenterAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductFeedbacksController : ControllerBase
    {
        private readonly IProductFeedbackService _productFeedbackService;

        public ProductFeedbacksController(IProductFeedbackService productFeedbackService)
        {
            _productFeedbackService = productFeedbackService;
        }

        // ============================================================
        // FEEDBACK — VIEW BY PRODUCT (CUSTOMER SIDE)
        // ============================================================
        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetFeedbacksByProductId(Guid productId)
        {
            try
            {
                var feedbacks = await _productFeedbackService.GetFeedbacksByProductIdAsync(productId);
                return Ok(new { success = true, data = feedbacks });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // ============================================================
        // FEEDBACK — VIEW BY ORDER (CUSTOMER SIDE — ORDER DETAIL POPUP)
        // ============================================================
        [HttpGet("order/{orderId}")]
        [Authorize]
        public async Task<IActionResult> GetFeedbacksByOrderId(Guid orderId)
        {
            try
            {
                var feedbacks = await _productFeedbackService.GetFeedbacksByOrderIdAsync(orderId);
                return Ok(new { success = true, data = feedbacks });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("check/{orderId}")]
        [Authorize]
        public async Task<IActionResult> CheckHasFeedback(Guid orderId)
        {
            try
            {
                var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (customerIdClaim == null || !Guid.TryParse(customerIdClaim, out var customerId))
                    return Unauthorized(new { success = false, message = "Invalid token." });

                var hasFeedback = await _productFeedbackService.HasFeedbackForOrderAsync(orderId, customerId);
                return Ok(new { success = true, hasFeedback });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
