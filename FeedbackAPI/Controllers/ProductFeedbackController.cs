using FeedbackAPI.DTOs;
using FeedbackAPI.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductFeedbackController : ControllerBase
    {
        private readonly IProductFeedbackService _service;

        public ProductFeedbackController(IProductFeedbackService service)
        {
            _service = service;
        }

        // GET: api/ProductFeedback/product/{productId}
        // Lấy tất cả feedback của 1 sản phẩm để hiển thị tại product detail
        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetFeedbacksByProductId(Guid productId)
        {
            try
            {
                var feedbacks = await _service.GetFeedbacksByProductIdAsync(productId);
                return Ok(new { success = true, data = feedbacks });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: api/ProductFeedback/order/{orderId}
        // Lấy tất cả feedback của 1 order để hiển thị popup View Feedback
        [HttpGet("order/{orderId}")]
        [Authorize]
        public async Task<IActionResult> GetFeedbacksByOrderId(Guid orderId)
        {
            try
            {
                var feedbacks = await _service.GetFeedbacksByOrderIdAsync(orderId);
                return Ok(new { success = true, data = feedbacks });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: api/ProductFeedback/check/{orderId}
        // Kiểm tra order đã feedback chưa → frontend dùng để hiển thị nút Write Feedback hay View Feedback
        [HttpGet("check/{orderId}")]
        [Authorize]
        public async Task<IActionResult> CheckHasFeedback(Guid orderId)
        {
            try
            {
                var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (customerIdClaim == null || !Guid.TryParse(customerIdClaim, out var customerId))
                    return Unauthorized(new { success = false, message = "Invalid token." });

                var hasFeedback = await _service.HasFeedbackForOrderAsync(orderId, customerId);
                return Ok(new { success = true, hasFeedback });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // POST: api/ProductFeedback/bulk
        // Tạo feedback cho nhiều sản phẩm trong 1 order cùng lúc
        [HttpPost("bulk")]
        [Authorize]
        public async Task<IActionResult> CreateBulkFeedback([FromForm] BulkFeedbackFormDto form)
        {
            try
            {
                var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (customerIdClaim == null || !Guid.TryParse(customerIdClaim, out var customerId))
                    return Unauthorized(new { success = false, message = "Invalid token." });

                var dto = MapFromForm(form, Request.Form.Files);

                var result = await _service.CreateBulkFeedbackAsync(customerId, dto);
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

        // PUT: api/ProductFeedback/update
        // Cập nhật feedback đã tạo
        [HttpPut("update")]
        [Authorize]
        public async Task<IActionResult> UpdateFeedback([FromBody] UpdateProductFeedbackDto dto)
        {
            try
            {
                var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (customerIdClaim == null || !Guid.TryParse(customerIdClaim, out var customerId))
                    return Unauthorized(new { success = false, message = "Invalid token." });

                var result = await _service.UpdateFeedbackAsync(customerId, dto);
                if (result == null)
                    return NotFound(new { success = false, message = "Feedback not found." });

                return Ok(new { success = true, data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // =================== HELPER ===================

        // Map form data + files sang CreateBulkFeedbackDto
        // Files được gắn theo index: Feedbacks[0].MediaFiles, Feedbacks[1].MediaFiles, ...
        private static CreateBulkFeedbackDto MapFromForm(
            BulkFeedbackFormDto form,
            IFormFileCollection allFiles)
        {
            var dto = new CreateBulkFeedbackDto();

            for (int i = 0; i < form.Feedbacks.Count; i++)
            {
                var f = form.Feedbacks[i];

                // Lấy files theo key Feedbacks[i].MediaFiles
                var fileKey = $"Feedbacks[{i}].MediaFiles";
                var files = allFiles.GetFiles(fileKey)?.ToList()
                            ?? new List<IFormFile>();

                dto.Feedbacks.Add(new CreateProductFeedbackDto
                {
                    ProductId = f.ProductId,
                    OrderId = f.OrderId,
                    Rating = f.Rating,
                    Comment = f.Comment,
                    MediaFiles = files
                });
            }

            return dto;
        }
    }
}
