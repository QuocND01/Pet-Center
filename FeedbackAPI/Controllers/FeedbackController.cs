using FeedbackAPI.DTOs;
using FeedbackAPI.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _service;

        public FeedbackController(IFeedbackService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFeedbackDTO dto)
        {
            await _service.CreateFeedbackAsync(dto);
            return Ok("Created successfully");
        }

        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetByProduct(Guid productId)
        {
            var result = await _service.GetByProductAsync(productId);
            return Ok(result);
        }

        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetByCustomer(Guid customerId)
        {
            var result = await _service.GetByCustomerAsync(customerId);
            return Ok(result);
        }

        [HttpGet("detail/{feedbackId}")]
        public async Task<IActionResult> GetDetail(Guid feedbackId)
        {
            var result = await _service.GetDetailAsync(feedbackId);

            if (result == null)
                return NotFound("Feedback not found");

            return Ok(result);
        }

        [HttpGet("admin/all")]
        public async Task<IActionResult> GetAllForAdmin()
        {
            var result = await _service.GetAllForAdminAsync();
            return Ok(result);
        }

        [HttpGet("admin/filter")]
        public async Task<IActionResult> Filter(
            int? rating,
            Guid? productId,
            bool? isVisible,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var result = await _service.FilterAsync(
                rating,
                productId,
                isVisible,
                fromDate,
                toDate);

            return Ok(result);
        }

        [HttpPut("reply/{feedbackId}")]
        public async Task<IActionResult> Reply(
            Guid feedbackId,
            [FromQuery] Guid staffId,
            [FromBody] string reply)
        {
            await _service.ReplyFeedbackAsync(feedbackId, staffId, reply);
            return Ok("Replied successfully");
        }


        [HttpPut("reply/update/{feedbackId}")]
        public async Task<IActionResult> UpdateReply(
            Guid feedbackId,
            [FromBody] string reply)
        {
            await _service.UpdateReplyAsync(feedbackId, reply);
            return Ok("Reply updated successfully");
        }

        [HttpDelete("reply/{feedbackId}")]
        public async Task<IActionResult> DeleteReply(Guid feedbackId)
        {
            await _service.DeleteReplyAsync(feedbackId);
            return Ok("Reply deleted successfully");
        }

        [HttpPut("admin/toggle-visibility/{feedbackId}")]
        public async Task<IActionResult> ToggleVisibility(Guid feedbackId)
        {
            await _service.ToggleVisibilityAsync(feedbackId);
            return Ok("Visibility updated");
        }

        [HttpDelete("{feedbackId}")]
        public async Task<IActionResult> Delete(Guid feedbackId)
        {
            await _service.DeleteFeedbackAsync(feedbackId);
            return Ok("Feedback deleted (soft delete)");
        }
    }
}
