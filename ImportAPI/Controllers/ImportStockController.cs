using PetCenterAPI.DTOs;
using PetCenterAPI.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace PetCenterAPI.Controllers
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

        // GET: api/importstock
        [HttpGet]
        public async Task<IActionResult> GetAllImports()
        {
            var imports = await _service.GetAllImportsAsync();

            return Ok(imports);
        }

        // POST: api/importstock
        [HttpPost]
        public async Task<IActionResult> Create(CreateImportStockDto dto)
        {
            // var staffClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // if (string.IsNullOrEmpty(staffClaim))
            // {
            //     return Unauthorized("StaffId missing in token");
            // }

            // var staffId = Guid.Parse(staffClaim);

            //=== TEST ===
            var staffId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            //============

            var id = await _service.CreateAsync(dto, staffId);

            return Ok(id);
        }

        // GET: api/importstock/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
            {
                return NotFound("Import stock not found");
            }

            return Ok(result);
        }

        // PUT: api/importstock/{id}/confirm
        [HttpPut("{id}/confirm")]
        public async Task<IActionResult> Confirm(Guid id)
        {
            var items = await _service.ConfirmAsync(id);

            return Ok(items);
        }

        // PUT: api/importstock/{id}/cancel
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            await _service.CancelAsync(id);

            return NoContent();
        }

        // GET: api/importstock/export
        [HttpGet("export")]
        public async Task<IActionResult> Export(
            DateTime? fromDate,
            DateTime? toDate)
        {
            var result = await _service.Export(fromDate, toDate);

            return Ok(result);
        }

        // POST: api/importstock/deduct
        [HttpPost("deduct")]
        public async Task<IActionResult> Deduct(DeductStockRequest req)
        {
            var mapping = await _service.DeductFIFO(
                req.ProductId,
                req.Quantity);

            return Ok(new DeductStockResponse
            {
                Mapping = mapping
            });
        }

        // POST: api/importstock/return
        [HttpPost("return")]
        public async Task<IActionResult> Return(ReturnStockRequest req)
        {
            await _service.ReturnStock(req.Mapping);

            return NoContent();
        }

        [HttpGet("check-product/{productId}")]
        public async Task<IActionResult> CheckProduct(Guid productId)
        {
            var exists = await _service
                .HasProductInImportsAsync(productId);

            return Ok(exists);
        }
    }
}