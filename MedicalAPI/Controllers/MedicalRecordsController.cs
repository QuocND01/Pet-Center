// File: Controllers/MedicalRecordsController.cs
using MedicalAPI.DTOs.MedicalRecord;
using MedicalAPI.DTOs.Prescription;
using MedicalAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using System.Globalization;

namespace MedicalAPI.Controllers;

[ApiController]
[Route("api/v1/medical-records")]
//[Authorize]
public class MedicalRecordsController : ControllerBase
{
    private readonly IMedicalRecordService _recordService;
    private readonly IPrescriptionItemService _prescriptionService;

    public MedicalRecordsController(
        IMedicalRecordService recordService,
        IPrescriptionItemService prescriptionService)
    {
        _recordService = recordService;
        _prescriptionService = prescriptionService;
    }

    private string Timestamp =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"))
            .ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

    // ─────────────────────────────────────────────
    // Medical Records Endpoints
    // ─────────────────────────────────────────────

    /// <summary>
    /// GET /api/v1/medical-records
    /// Supports OData: $filter, $select, $orderby, $top, $skip, $count
    /// </summary>
    [HttpGet]
    [EnableQuery(MaxTop = 100, PageSize = 10)]
    public ActionResult<IQueryable<MedicalRecordReadDto>> GetAll()
    {
        try
        {
            var query = _recordService.GetQueryable();
            return Ok(query);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "An unexpected error occurred.",
                data = (object?)null,
                errors = new[] { ex.Message },
                timestamp = Timestamp
            });
        }
    }

    /// <summary>GET /api/v1/medical-records/{id}</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var record = await _recordService.GetByIdAsync(id);
            return Ok(new { success = true, message = "Medical record retrieved successfully.", data = record, errors = (object?)null, timestamp = Timestamp });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message, data = (object?)null, errors = new[] { ex.Message }, timestamp = Timestamp });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An unexpected error occurred.", data = (object?)null, errors = new[] { ex.Message }, timestamp = Timestamp });
        }
    }

    /// <summary>GET /api/v1/medical-records/by-appointment/{appointmentId}</summary>
    [HttpGet("by-appointment/{appointmentId:guid}")]
    public async Task<IActionResult> GetByAppointmentId(Guid appointmentId)
    {
        try
        {
            var records = await _recordService.GetByAppointmentIdAsync(appointmentId);
            return Ok(new { success = true, message = "Medical records retrieved successfully.", data = records, errors = (object?)null, timestamp = Timestamp });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An unexpected error occurred.", data = (object?)null, errors = new[] { ex.Message }, timestamp = Timestamp });
        }
    }

    /// <summary>POST /api/v1/medical-records</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MedicalRecordCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
            return BadRequest(new { success = false, message = "Validation failed.", data = (object?)null, errors, timestamp = Timestamp });
        }

        try
        {
            var created = await _recordService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.RecordId },
                new { success = true, message = "Medical record created successfully.", data = created, errors = (object?)null, timestamp = Timestamp });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message, data = (object?)null, errors = new[] { ex.Message }, timestamp = Timestamp });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message, data = (object?)null, errors = new[] { ex.Message }, timestamp = Timestamp });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An unexpected error occurred.", data = (object?)null, errors = new[] { ex.Message }, timestamp = Timestamp });
        }
    }

    /// <summary>PUT /api/v1/medical-records/{id}</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] MedicalRecordUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
            return BadRequest(new { success = false, message = "Validation failed.", data = (object?)null, errors, timestamp = Timestamp });
        }

        try
        {
            var updated = await _recordService.UpdateAsync(id, dto);
            return Ok(new { success = true, message = "Medical record updated successfully.", data = updated, errors = (object?)null, timestamp = Timestamp });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message, data = (object?)null, errors = new[] { ex.Message }, timestamp = Timestamp });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message, data = (object?)null, errors = new[] { ex.Message }, timestamp = Timestamp });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An unexpected error occurred.", data = (object?)null, errors = new[] { ex.Message }, timestamp = Timestamp });
        }
    }

    /// <summary>PATCH /api/v1/medical-records/{id}/status</summary>
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] MedicalRecordStatusUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
            return BadRequest(new { success = false, message = "Validation failed.", data = (object?)null, errors, timestamp = Timestamp });
        }

        try
        {
            var updated = await _recordService.UpdateStatusAsync(id, dto);
            return Ok(new { success = true, message = "Medical record status updated successfully.", data = updated, errors = (object?)null, timestamp = Timestamp });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message, data = (object?)null, errors = new[] { ex.Message }, timestamp = Timestamp });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message, data = (object?)null, errors = new[] { ex.Message }, timestamp = Timestamp });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An unexpected error occurred.", data = (object?)null, errors = new[] { ex.Message }, timestamp = Timestamp });
        }
    }

    /// <summary>DELETE /api/v1/medical-records/{id} — Soft delete (set status = Draft)</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _recordService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message, data = (object?)null, errors = new[] { ex.Message }, timestamp = Timestamp });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An unexpected error occurred.", data = (object?)null, errors = new[] { ex.Message }, timestamp = Timestamp });
        }
    }

    // ─────────────────────────────────────────────
    // Prescription Items — nested under record
    // ─────────────────────────────────────────────

    /// <summary>GET /api/v1/medical-records/{recordId}/prescriptions</summary>
    [HttpGet("{recordId:guid}/prescriptions")]
    public async Task<IActionResult> GetPrescriptions(Guid recordId)
    {
        try
        {
            var items = await _prescriptionService.GetByRecordIdAsync(recordId);
            return Ok(new { success = true, message = "Prescriptions retrieved successfully.", data = items, errors = (object?)null, timestamp = Timestamp });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message, data = (object?)null, errors = new[] { ex.Message }, timestamp = Timestamp });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An unexpected error occurred.", data = (object?)null, errors = new[] { ex.Message }, timestamp = Timestamp });
        }
    }

    /// <summary>POST /api/v1/medical-records/{recordId}/prescriptions</summary>
    [HttpPost("{recordId:guid}/prescriptions")]
    public async Task<IActionResult> AddPrescription(Guid recordId, [FromBody] PrescriptionItemCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
            return BadRequest(new { success = false, message = "Validation failed.", data = (object?)null, errors, timestamp = Timestamp });
        }

        try
        {
            var created = await _prescriptionService.CreateAsync(recordId, dto);
            return CreatedAtAction(nameof(GetPrescriptionById), new { id = created.PrescriptionItemId },
                new { success = true, message = "Prescription item added successfully.", data = created, errors = (object?)null, timestamp = Timestamp });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message, data = (object?)null, errors = new[] { ex.Message }, timestamp = Timestamp });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message, data = (object?)null, errors = new[] { ex.Message }, timestamp = Timestamp });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An unexpected error occurred.", data = (object?)null, errors = new[] { ex.Message }, timestamp = Timestamp });
        }
    }

    // ─────────────────────────────────────────────
    // Prescription Items — standalone
    // ─────────────────────────────────────────────

    /// <summary>GET /api/v1/prescriptions/{id}</summary>
    [HttpGet("/api/v1/prescriptions/{id:guid}", Name = "GetPrescriptionById")]
    public async Task<IActionResult> GetPrescriptionById(Guid id)
    {
        try
        {
            var item = await _prescriptionService.GetByIdAsync(id);
            return Ok(new { success = true, message = "Prescription item retrieved successfully.", data = item, errors = (object?)null, timestamp = Timestamp });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message, data = (object?)null, errors = new[] { ex.Message }, timestamp = Timestamp });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An unexpected error occurred.", data = (object?)null, errors = new[] { ex.Message }, timestamp = Timestamp });
        }
    }

    /// <summary>PUT /api/v1/prescriptions/{id}</summary>
    [HttpPut("/api/v1/prescriptions/{id:guid}")]
    public async Task<IActionResult> UpdatePrescription(Guid id, [FromBody] PrescriptionItemUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
            return BadRequest(new { success = false, message = "Validation failed.", data = (object?)null, errors, timestamp = Timestamp });
        }

        try
        {
            var updated = await _prescriptionService.UpdateAsync(id, dto);
            return Ok(new { success = true, message = "Prescription item updated successfully.", data = updated, errors = (object?)null, timestamp = Timestamp });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message, data = (object?)null, errors = new[] { ex.Message }, timestamp = Timestamp });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message, data = (object?)null, errors = new[] { ex.Message }, timestamp = Timestamp });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An unexpected error occurred.", data = (object?)null, errors = new[] { ex.Message }, timestamp = Timestamp });
        }
    }

    /// <summary>DELETE /api/v1/prescriptions/{id}</summary>
    [HttpDelete("/api/v1/prescriptions/{id:guid}")]
    public async Task<IActionResult> DeletePrescription(Guid id)
    {
        try
        {
            await _prescriptionService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message, data = (object?)null, errors = new[] { ex.Message }, timestamp = Timestamp });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message, data = (object?)null, errors = new[] { ex.Message }, timestamp = Timestamp });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An unexpected error occurred.", data = (object?)null, errors = new[] { ex.Message }, timestamp = Timestamp });
        }
    }
}