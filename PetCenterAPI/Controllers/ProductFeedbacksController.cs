using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PetCenterAPI.Service.Interface;
using PetCenterAPI.DTOs.Requests.ManageFeedback;

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

        // ============================================================
        // FEEDBACK — CREATE BULK (CUSTOMER SIDE)
        // ============================================================
        [HttpPost("bulk")]
        [Authorize]
        public async Task<IActionResult> CreateBulkFeedback([FromForm] BulkFeedbackFormRequestDTO form)
        {
            try
            {
                var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (customerIdClaim == null || !Guid.TryParse(customerIdClaim, out var customerId))
                    return Unauthorized(new { success = false, message = "Invalid token." });

                var request = MapFromForm(form, Request.Form.Files);
                var result = await _productFeedbackService.CreateBulkFeedbackAsync(customerId, request);
                return Ok(new { success = true, data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // ============================================================
        // FEEDBACK — HELPER
        // ============================================================
        private static CreateBulkFeedbackRequestDTO MapFromForm(
    BulkFeedbackFormRequestDTO form,
    IFormFileCollection allFiles)
        {
            var request = new CreateBulkFeedbackRequestDTO();
            for (int i = 0; i < form.Feedbacks.Count; i++)
            {
                var f = form.Feedbacks[i];
                var fileKey = $"Feedbacks[{i}].MediaFiles";
                var files = allFiles.GetFiles(fileKey)?.ToList() ?? new List<IFormFile>();
                request.Feedbacks.Add(new CreateFeedbackItemRequestDTO
                {
                    ProductId = f.ProductId,
                    OrderId = f.OrderId,
                    Rating = f.Rating,
                    Comment = f.Comment,
                    MediaFiles = files
                });
            }
            return request;
        }
    }
}
