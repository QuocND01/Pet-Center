// File: Controllers/StaffsController.cs
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using StaffAPI.DTOs.Role;
using StaffAPI.DTOs.Staff;
using StaffAPI.Services.Interfaces;

namespace StaffAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class StaffsController : ODataController
{
    private readonly IStaffService _staffService;
    private readonly IMapper _mapper;

    public StaffsController(IStaffService staffService, IMapper mapper)
    {
        _staffService = staffService;
        _mapper = mapper;
    }

    private static string Timestamp =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"))
            .ToString("dd/MM/yyyy HH:mm");

    private static object Wrap(bool success, string message, object? data = null, IEnumerable<string>? errors = null)
        => new { success, message, data, errors, timestamp = Timestamp };

    /// <summary>Get paginated staff list. Supports OData $filter, $select, $orderby, $top, $skip, $count.</summary>
    [HttpGet]
    [EnableQuery(MaxTop = 100, PageSize = 10)]
    public IActionResult GetAll()
    {
        try
        {
            var q = _staffService.GetAllQueryable()
                .Select(s => new StaffReadDto
                {
                    StaffId = s.StaffId,
                    FullName = s.FullName,
                    PhoneNumber = s.PhoneNumber,
                    BirthDate = s.BirthDate,
                    Gender = s.Gender,
                    HireDate = s.HireDate,
                    Email = s.Email,
                    Avatar = s.Avatar,
                    IsActive = s.IsActive,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    Roles = s.Roles.Select(r => new RoleReadDto
                    {
                        RoleId = r.RoleId,
                        RoleName = r.RoleName
                    }).ToList(),
                    VetProfile = s.VetProfile == null ? null : new DTOs.VetProfile.VetProfileReadDto
                    {
                        VetProfileId = s.VetProfile.VetProfileId,
                        StaffId = s.VetProfile.StaffId,
                        ExperienceYears = s.VetProfile.ExperienceYears,
                        Description = s.VetProfile.Description,
                        LicenseNumber = s.VetProfile.LicenseNumber,
                        IsActive = s.VetProfile.IsActive
                    }
                });

            return Ok(q);
        }
        catch (Exception ex)
        {
            return StatusCode(500, Wrap(false, ex.Message));
        }
    }

    /// <summary>Get staff by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var dto = await _staffService.GetByIdAsync(id);
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

    /// <summary>Create a new staff member. If role is Vet, VetProfile data is required.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] StaffCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(Wrap(false, "Validation failed.", null, errors));
        }

        try
        {
            var created = await _staffService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.StaffId },
                Wrap(true, "Staff created successfully.", created));
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

    /// <summary>Update staff information.</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] StaffUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(Wrap(false, "Validation failed.", null, errors));
        }

        try
        {
            var updated = await _staffService.UpdateAsync(id, dto);
            return Ok(Wrap(true, "Staff updated successfully.", updated));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(Wrap(false, ex.Message));
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

    /// <summary>Soft-delete a staff member (IsActive = false).</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _staffService.SoftDeleteAsync(id);
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