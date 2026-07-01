using Microsoft.AspNetCore.Mvc;
using PetCenterAPI.DTOs.Requests.Inventory;
using PetCenterAPI.Service.Interface;

namespace PetCenterAPI.Controllers
{
    [Route("api/inventories")]
    [ApiController]
    public class InventoriesController : ControllerBase
    {
        private readonly IInventoryService _service;

        public InventoriesController(IInventoryService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get inventory list
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPaged(
            [FromQuery] InventoryQueryRequestDTO request)
        {
            var result = await _service.GetPagedAsync(request);

            return Ok(result);
        }

        /// <summary>
        /// Get inventory detail
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
            {
                return NotFound(new
                {
                    Message = "Inventory not found."
                });
            }

            return Ok(result);
        }
    }
}
