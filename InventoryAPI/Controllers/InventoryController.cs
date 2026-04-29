using InventoryAPI.DTOs;
using InventoryAPI.Models;
using InventoryAPI.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

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
        public async Task<ActionResult<IEnumerable<ProductQuantityDTO>>> GetProductStocks([FromBody] List<Guid> productIds)
        {
            var stocks = await _inventoryService.GetProductStockAsync(productIds);
            return Ok(stocks);
        }

        
        [HttpGet]
        
        public IActionResult Get()
        {
            return Ok(_inventoryService.GetInventories());
        }

        // View Inventory by Id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _inventoryService.GetInventoryById(id);
            if (data == null) return NotFound();
            return Ok(data);
        }
    }
}
