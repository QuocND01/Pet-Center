using Microsoft.AspNetCore.Mvc;
using PetCenterAPI.Common;
using PetCenterAPI.Service.Interface;
using static PetCenterAPI.DTOs.Requests.MedicalRecord.MedicalRecordRequestDTO;

namespace PetCenterAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicalRecordsController : ControllerBase
    {
        private readonly IMedicalRecordService _service;

        public MedicalRecordsController(IMedicalRecordService service)
        {
            _service = service;
        }

        [HttpGet("admin")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] int? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var (items, total) = await _service.GetAllAsync(search, status, page, pageSize);
            return Ok(new
            {
                Data = items,
                TotalCount = total,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)total / pageSize)
            });
        }

        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetByCustomer(
            Guid customerId,
            [FromQuery] string? search)
        {
            var items = await _service.GetByCustomerIdAsync(customerId, search);
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var record = await _service.GetByIdAsync(id);
            if (record == null) return NotFound();
            return Ok(record);
        }

        [HttpGet("completed-appointments")]
        public async Task<IActionResult> GetCompletedAppointments()
        {
            var list = await _service.GetCompletedAppointmentsAsync();
            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMedicalRecordDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _service.CreateAsync(dto);
                return Ok(new { success = true, message = "Medical record created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMedicalRecordDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _service.UpdateAsync(id, dto);
                return Ok(new { success = true, message = "Medical record updated successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] MedicalRecordStatus status)
        {
            try
            {
                await _service.ChangeStatusAsync(id, status);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
