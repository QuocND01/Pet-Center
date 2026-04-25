// File: Controllers/VetProfilesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using StaffAPI.DTOs.Staff;
using StaffAPI.DTOs.VetProfile;
using StaffAPI.Services.Interfaces;

namespace StaffAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class VetProfilesController : ODataController
{
    private readonly IVetProfileService _vetService;

    public VetProfilesController(IVetProfileService vetService)
    {
        _vetService = vetService;
    }

    private static string Timestamp =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"))
            .ToString("dd/MM/yyyy HH:mm");

    private static object Wrap(bool success, string message, object? data = null, IEnumerable<string>? errors = null)
        => new { success, message, data, errors, timestamp = Timestamp };

    /// <summary>Get all vet profiles. Supports OData queries.</summary>
    [HttpGet]
    [EnableQuery(MaxTop = 100, PageSize = 10)]
    public IActionResult GetAll()
    {
        try
        {
            var q = _vetService.GetAllQueryable()
                .Select(v => new VetProfileReadDto
                {
                    VetProfileId = v.VetProfileId,
                    StaffId = v.StaffId,
                    ExperienceYears = v.ExperienceYears,
                    Description = v.Description,
                    LicenseNumber = v.LicenseNumber,
                    IsActive = v.IsActive
                });

            return Ok(q);
        }
        catch (Exception ex)
        {
            return StatusCode(500, Wrap(false, ex.Message));
        }
    }

    /// <summary>Get vet profile by VetProfileId.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var dto = await _vetService.GetByIdAsync(id);
            return Ok(Wrap(true, "Success", dto));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(Wrap(false, ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, Wrap(false, ex.Message));
        }
    }

    /// <summary>Get vet profile by StaffId.</summary>
    [HttpGet("by-staff/{staffId:guid}")]
    public async Task<IActionResult> GetByStaffId(Guid staffId)
    {
        try
        {
            var dto = await _vetService.GetByStaffIdAsync(staffId);
            return Ok(Wrap(true, "Success", dto));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(Wrap(false, ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, Wrap(false, ex.Message));
        }
    }

    /// <summary>Add a VetProfile to an existing staff member.</summary>
    [HttpPost("staff/{staffId:guid}")]
    public async Task<IActionResult> Create(Guid staffId, [FromBody] VetProfileCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(Wrap(false, "Validation failed.", null, errors));
        }

        try
        {
            var created = await _vetService.CreateAsync(staffId, dto);
            return CreatedAtAction(nameof(GetById), new { id = created.VetProfileId },
                Wrap(true, "VetProfile created successfully.", created));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(Wrap(false, ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(Wrap(false, ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(Wrap(false, ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, Wrap(false, ex.Message));
        }
    }

    /// <summary>
    /// Update VetProfile. Chỉ cho phép cập nhật Description và IsActive.
    /// ExperienceYears, LicenseNumber, Rating không được cập nhật qua endpoint này.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] VetProfileUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(Wrap(false, "Validation failed.", null, errors));
        }

        try
        {
            var updated = await _vetService.UpdateAsync(id, dto);
            return Ok(Wrap(true, "VetProfile updated successfully.", updated));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(Wrap(false, ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, Wrap(false, ex.Message));
        }
    }

    /// <summary>Soft-delete VetProfile (IsActive = false).</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _vetService.SoftDeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(Wrap(false, ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, Wrap(false, ex.Message));
        }
    }
}