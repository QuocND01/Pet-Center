using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using PetCenterAPI.Service.Interface;
using static PetCenterAPI.DTOs.Requests.Order.OrderRequestDTO;

namespace PetCenterAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // 👇 ĐỔI TÊN HÀM VÀ ROUTE Ở ĐÂY
        // [Authorize(Roles = "Admin, Sale")] // Bỏ comment khi ráp bảo mật
        [HttpGet] // <-- Bỏ chữ "admin" đi để OData tự động nhận diện
        public async Task<IActionResult> Get(ODataQueryOptions<ReadOrderListDTO> queryOptions)
        {
            try
            {
                var result = await _orderService.GetOrderListAdminAsync(queryOptions);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: api/Orders/{id}
        [HttpGet("{id:guid}")]
        [AllowAnonymous] // Tạm mở toang để test UI cho lẹ, sau này ráp Auth vào nhé
        public async Task<IActionResult> GetOrderDetails(Guid id)
        {
            try
            {
                var order = await _orderService.GetOrderDetailsAsync(id);
                if (order == null)
                {
                    return NotFound(new { message = "Order not found" });
                }
                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // PATCH: api/Orders/{id}/cancel
        [HttpPatch("{id:guid}/cancel")]
        [AllowAnonymous]
        public async Task<IActionResult> CancelOrder(Guid id)
        {
            try
            {
                await _orderService.CancelOrderAsync(id);
                return Ok(new { success = true, message = "Order cancelled successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // PATCH: api/Orders/{id}/advance
        [HttpPatch("{id:guid}/advance")]
        [AllowAnonymous]
        public async Task<IActionResult> AdvanceStatus(Guid id)
        {
            try
            {
                var newStatus = await _orderService.AdvanceOrderStatusAsync(id);
                return Ok(new { success = true, status = newStatus, message = "Order status updated successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        //[Authorize(Roles = "Customer")]
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            try
            {
                // Lấy CustomerID an toàn từ Token mà AuthsController đã cấp
                var customerIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(customerIdStr, out var customerId))
                {
                    return Unauthorized(new { success = false, message = "Invalid token data." });
                }

                var orders = await _orderService.GetCustomerOrderHistoryAsync(customerId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}