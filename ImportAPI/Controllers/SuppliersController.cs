using PetCenterAPI.DTOs;
using PetCenterAPI.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace PetCenterAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuppliersController : ControllerBase
    {
        private readonly ISupplierService _service;

        public SuppliersController(ISupplierService service)
        {
            _service = service;
        }

        // GET: api/suppliers
        [HttpGet]
        public async Task<IActionResult> GetSuppliers()
        {
            var result = await _service.GetAllAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Get suppliers successfully",
                Data = result
            });
        }

        // GET: api/suppliers/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetSupplierById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Supplier not found"
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Get supplier successfully",
                Data = result
            });
        }

        // POST: api/suppliers
        [HttpPost]
        public async Task<IActionResult> CreateSupplier([FromBody] WriteSupplierDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = ModelState
                });
            }

            var createdSupplier = await _service.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetSupplierById),
                new { id = createdSupplier.SupplierId },
                new ApiResponse<object>
                {
                    Success = true,
                    Message = "Create supplier successfully",
                    Data = createdSupplier
                });
        }

        // PUT: api/suppliers/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateSupplier(Guid id, [FromBody] WriteSupplierDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = ModelState
                });
            }

            var success = await _service.UpdateAsync(id, dto);

            if (!success)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Supplier not found"
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Update supplier successfully"
            });
        }

        // DELETE: api/suppliers/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteSupplier(Guid id)
        {
            var success = await _service.DeleteAsync(id);

            if (!success)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Supplier not found"
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Delete supplier successfully"
            });
        }
    }
}