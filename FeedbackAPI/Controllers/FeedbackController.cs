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
        public async Task<IActionResult> Create(CreateFeedbackDTO dto)
        {
            await _service.CreateFeedbackAsync(dto);
            return Ok("Created successfully");
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetByProduct(Guid productId)
        {
            var result = await _service.GetByProductAsync(productId);
            return Ok(result);
        }

        [HttpPut("reply/{feedbackId}")]
        public async Task<IActionResult> Reply(Guid feedbackId, string reply)
        {
            await _service.ReplyFeedbackAsync(feedbackId, reply);
            return Ok("Replied successfully");
        }
    }
}
