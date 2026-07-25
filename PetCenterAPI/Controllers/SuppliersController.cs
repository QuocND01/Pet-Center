using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PetCenterAPI.DTOs.Requests.Supplier;
using PetCenterAPI.Service.Interface;
using System;
using System.Threading.Tasks;

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
                status = true, // Thành công -> true
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
                    status = false, // Thất bại -> false
                    message = "Supplier not found"
                });
            }

            return Ok(new
            {
                status = true, // Thành công -> true
                message = "Get supplier successfully",
                data = supplier
            });
        }

        // POST: api/suppliers
        [HttpPost]
        public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierRequestDTO dto)
        {
            // 1. Kiểm tra validation từ Data Annotations (DTO)
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    status = false,
                    message = "Validation failed",
                    errors = ModelState
                });
            }

            try
            {
                // 2. Gọi Service xử lý logic
                var supplier = await _service.CreateAsync(dto);

                // 3. Trả về Response 201 Created thành công
                return CreatedAtAction(
                    nameof(GetSupplierById),
                    new { id = supplier.SupplierId },
                    new
                    {
                        status = true,
                        message = "Create supplier successfully",
                        data = supplier
                    });
            }
            catch (InvalidOperationException ex)
            {
                // 4. Bắt lỗi nghiệp vụ từ Service (như trùng TaxId) -> Trả về 400 Bad Request
                return BadRequest(new
                {
                    status = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                // 5. Bắt các lỗi hệ thống không lường trước -> Trả về 500 Internal Server Error
                return StatusCode(500, new
                {
                    status = false,
                    message = "An error occurred while creating the supplier."
                });
            }
        }

        // PUT: api/suppliers/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateSupplier(
            Guid id,
            [FromBody] UpdateSupplierRequestDTO dto)
        {
            // 1. Validate dữ liệu đầu vào
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    status = false,
                    message = "Validation failed",
                    errors = ModelState
                });
            }

            try
            {
                // 2. Gọi Service thực hiện Cập nhật
                var success = await _service.UpdateAsync(id, dto);

                if (!success)
                {
                    return NotFound(new
                    {
                        status = false,
                        message = "Supplier not found"
                    });
                }

                return Ok(new
                {
                    status = true,
                    message = "Update supplier successfully"
                });
            }
            catch (InvalidOperationException ex)
            {
                // 3. Bắt lỗi nghiệp vụ từ Service (VD: TaxId bị trùng với nhà cung cấp KHÁC)
                return BadRequest(new
                {
                    status = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                // 4. Bắt lỗi hệ thống không lường trước
                return StatusCode(500, new
                {
                    status = false,
                    message = "An error occurred while updating the supplier."
                });
            }
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
                    status = false, // Thất bại -> false
                    message = "Supplier not found"
                });
            }

            return Ok(new
            {
                status = true, // Thành công -> true
                message = "Delete supplier successfully"
            });
        }
    }
}