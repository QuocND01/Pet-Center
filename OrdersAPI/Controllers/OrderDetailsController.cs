using Microsoft.AspNetCore.Mvc;
using OrdersAPI.DTOs;
using OrdersAPI.Service.Interface;

namespace OrdersAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderDetailsController : ControllerBase
    {
        private readonly IOrderDetailService _service;

        public OrderDetailsController(IOrderDetailService service)
        {
            _service = service;
        }

        // 1. LẤY TẤT CẢ CHI TIẾT CỦA MỘT ĐƠN HÀNG (GET: api/OrderDetails/order/{orderId})
        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetByOrder(Guid orderId)
        {
            var result = await _service.GetDetailsByOrderAsync(orderId);
            return Ok(result);
        }

        // 2. THÊM MỚI CHI TIẾT (POST: api/OrderDetails)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OrderDetailRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _service.CreateDetailAsync(dto);
            if (!success)
                return BadRequest(new { message = "Không thể thêm chi tiết đơn hàng. Kiểm tra lại OrderId hoặc ProductId." });

            return Ok(new { message = "Thêm thành công" });
        }

        // 3. CẬP NHẬT CHI TIẾT (PUT: api/OrderDetails/{id})
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] OrderDetailRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _service.UpdateDetailAsync(id, dto);
            if (!success)
                return NotFound(new { message = "Không tìm thấy chi tiết đơn hàng để cập nhật." });

            return Ok(new { message = "Cập nhật thành công" });
        }

        // 4. XÓA CHI TIẾT (DELETE: api/OrderDetails/{id})
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteDetailAsync(id);
            if (!success)
                return NotFound(new { message = "Không tìm thấy chi tiết đơn hàng để xóa." });

            return Ok(new { message = "Xóa thành công" });
        }
    }
}