using InventoryAPI.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace InventoryAPI.Controllers
{
    [Route("api/inventory-transactions")]
    [ApiController]
    public class InventoryTransactionController : ControllerBase
    {
        private readonly IInventoryTransactionService _service;

        public InventoryTransactionController(IInventoryTransactionService service)
        {
            _service = service;
        }

        
        [HttpGet]
        
        public IActionResult Get()
        {
            return Ok(_service.GetTransactions());
        }

        // View transaction by inventory
        [HttpGet("by-inventory/{inventoryId}")]
        public async Task<IActionResult> GetByInventory(Guid inventoryId)
        {
            var data = await _service.GetByInventoryId(inventoryId);
            return Ok(data);
        }
    }
}
