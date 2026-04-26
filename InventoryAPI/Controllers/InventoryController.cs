using InventoryAPI.DTOs;
using InventoryAPI.Models;
using InventoryAPI.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InventoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService context)
        {
            _inventoryService = context;
        }

        [HttpPost("stocks")]
        public async Task<ActionResult<IEnumerable<ProductInventoryDTO>>> GetProductStocks([FromBody] List<Guid> productIds)
        {
            var stocks = await _inventoryService.GetProductStockAsync(productIds);
            return Ok(stocks);
        }
    }
}
