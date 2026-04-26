using ImportAPI.DTOs;
using ImportAPI.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ImportAPI.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ImportStockController : ControllerBase
    {
        private readonly IImportStockService _service;

        public ImportStockController(IImportStockService service)
        {
            _service = service;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAllImports()
        {
            var imports = await _service.GetAllImportsAsync();
            return Ok(imports);
        }
        
        [HttpPost]
        public async Task<IActionResult> Create(CreateImportStockDto dto)
        {
            var staffClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(staffClaim))
            {
                return Unauthorized("staffId missing in token");
            }

            var staffId = Guid.Parse(staffClaim);

            var id = await _service.CreateAsync(dto, staffId);

            return Ok(id);
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound();

            return Ok(result);
        }

        [HttpPut("{id}/confirm")]
        public async Task<IActionResult> Confirm(Guid id)
        {
            var items = await _service.ConfirmAsync(id);
            return Ok(items);
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            await _service.CancelAsync(id);
            return NoContent();
        }
        [HttpGet("export")]
        public async Task<IActionResult> Export(DateTime? fromDate, DateTime? toDate)
        {
            var result = await _service.Export(fromDate, toDate);
            return Ok(result);
        }

        [HttpPost("deduct")]
        public async Task<IActionResult> Deduct(DeductStockRequest req)
        {
            var mapping = await _service.DeductFIFO(req.ProductId, req.Quantity);

            return Ok(new DeductStockResponse
            {
                Mapping = mapping
            });
        }

        [HttpPost("return")]
        public async Task<IActionResult> Return(ReturnStockRequest req)
        {
            await _service.ReturnStock(req.Mapping);
            return Ok();
        }
    }
}