using Microsoft.AspNetCore.Mvc;
using PetCenterAPI.Service.Interface;

namespace PetCenterAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatbotsController : ControllerBase
    {
        private readonly IChatbotService _chatbotService;

        public ChatbotsController(IChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        // ============================================================
        // GET api/chatbots/orders/pending?customerId=...
        // ============================================================
        [HttpGet("orders/pending")]
        public async Task<IActionResult> GetPendingOrders([FromQuery] Guid customerId)
        {
            if (customerId == Guid.Empty)
                return BadRequest(new { message = "customerId không hợp lệ." });

            var orders = await _chatbotService.GetPendingOrdersAsync(customerId);
            return Ok(orders);
        }

        // ============================================================
        // GET api/chatbots/orders/latest-status?customerId=...
        // ============================================================
        [HttpGet("orders/latest-status")]
        public async Task<IActionResult> GetLatestOrderStatus([FromQuery] Guid customerId)
        {
            if (customerId == Guid.Empty)
                return BadRequest(new { message = "customerId không hợp lệ." });

            var order = await _chatbotService.GetLatestOrderStatusAsync(customerId);
            if (order is null)
                return NotFound(new { message = "Không tìm thấy đơn hàng nào." });

            return Ok(order);
        }
    }
}
