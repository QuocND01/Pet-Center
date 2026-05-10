using ImportAPI.DTOs;
using ImportAPI.Service.Interface;
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

        // GET: api/importstock
        [HttpGet]
        public async Task<IActionResult> GetAllImports()
        {
            var imports = await _service.GetAllImportsAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Get import stocks successfully",
                Data = imports
            });
        }

        // POST: api/importstock
        [HttpPost]
        public async Task<IActionResult> Create(CreateImportStockDto dto)
        {
            // var staffClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // if (string.IsNullOrEmpty(staffClaim))
            // {
            //     return Unauthorized(new ApiResponse<object>
            //     {
            //         Success = false,
            //         Message = "StaffId missing in token"
            //     });
            // }

            // var staffId = Guid.Parse(staffClaim);

            //=== TEST ===
            var staffId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            //============

            var id = await _service.CreateAsync(dto, staffId);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Create import stock successfully",
                Data = id
            });
        }

        // GET: api/importstock/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Import stock not found"
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Get import stock successfully",
                Data = result
            });
        }

        // PUT: api/importstock/{id}/confirm
        [HttpPut("{id}/confirm")]
        public async Task<IActionResult> Confirm(Guid id)
        {
            var items = await _service.ConfirmAsync(id);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Confirm import stock successfully",
                Data = items
            });
        }

        // PUT: api/importstock/{id}/cancel
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            await _service.CancelAsync(id);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Cancel import stock successfully"
            });
        }

        // GET: api/importstock/export
        [HttpGet("export")]
        public async Task<IActionResult> Export(DateTime? fromDate, DateTime? toDate)
        {
            var result = await _service.Export(fromDate, toDate);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Export import stock successfully",
                Data = result
            });
        }

        // POST: api/importstock/deduct
        [HttpPost("deduct")]
        public async Task<IActionResult> Deduct(DeductStockRequest req)
        {
            var mapping = await _service.DeductFIFO(req.ProductId, req.Quantity);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Deduct stock successfully",
                Data = new DeductStockResponse
                {
                    Mapping = mapping
                }
            });
        }

        // POST: api/importstock/return
        [HttpPost("return")]
        public async Task<IActionResult> Return(ReturnStockRequest req)
        {
            await _service.ReturnStock(req.Mapping);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Return stock successfully"
            });
        }
    }
}