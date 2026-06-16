using Microsoft.AspNetCore.Mvc;
using PetCenterAPI.DTOs.Responses.Supplier;
using PetCenterAPI.Service.Interface;

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
            var suppliers = await _service.GetAllAsync();

            return Ok(new
            {
                status = StatusCodes.Status200OK,
                message = "Get suppliers successfully",
                data = suppliers
            });
        }

        // GET: api/suppliers/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetSupplierById(Guid id)
        {
            var supplier = await _service.GetByIdAsync(id);

            if (supplier == null)
            {
                return NotFound(new
                {
                    status = StatusCodes.Status404NotFound,
                    message = "Supplier not found"
                });
            }

            return Ok(new
            {
                status = StatusCodes.Status200OK,
                message = "Get supplier successfully",
                data = supplier
            });
        }

        // POST: api/suppliers
        [HttpPost]
        public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierRequestDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    status = StatusCodes.Status400BadRequest,
                    message = "Validation failed",
                    errors = ModelState
                });
            }

            var supplier = await _service.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetSupplierById),
                new { id = supplier.SupplierId },
                new
                {
                    status = StatusCodes.Status201Created,
                    message = "Create supplier successfully",
                    data = supplier
                });
        }

        // PUT: api/suppliers/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateSupplier(
            Guid id,
            [FromBody] CreateSupplierRequestDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    status = StatusCodes.Status400BadRequest,
                    message = "Validation failed",
                    errors = ModelState
                });
            }

            var success = await _service.UpdateAsync(id, dto);

            if (!success)
            {
                return NotFound(new
                {
                    status = StatusCodes.Status404NotFound,
                    message = "Supplier not found"
                });
            }

            return Ok(new
            {
                status = StatusCodes.Status200OK,
                message = "Update supplier successfully"
            });
        }

        // DELETE: api/suppliers/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteSupplier(Guid id)
        {
            var success = await _service.DeleteAsync(id);

            if (!success)
            {
                return NotFound(new
                {
                    status = StatusCodes.Status404NotFound,
                    message = "Supplier not found"
                });
            }

            return Ok(new
            {
                status = StatusCodes.Status200OK,
                message = "Delete supplier successfully"
            });
        }
    }
}