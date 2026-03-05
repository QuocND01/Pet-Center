using Microsoft.AspNetCore.Mvc;
using OrdersAPI.DTOs;
using OrdersAPI.Service.Interface;

namespace OrdersAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _service;
        public OrdersController(IOrderService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllOrdersAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var order = await _service.GetOrderByIdAsync(id);
            return order == null ? NotFound() : Ok(order);
        }

        [HttpPost]
        public async Task<IActionResult> Create(OrderRequestDTO dto)
        {
            var result = await _service.CreateOrderAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.OrderId }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, OrderRequestDTO dto)
        {
            var success = await _service.UpdateOrderAsync(id, dto);
            return success ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteOrderAsync(id);
            return success ? Ok("Xóa đơn hàng thành công") : NotFound();
        }
    }
}
