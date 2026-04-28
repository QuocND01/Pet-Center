using CustomerAPI.DTOs.Pet;
using CustomerAPI.Models;
using CustomerAPI.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace CustomerAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PetsController : ODataController
{
    private readonly IPetService _petService;

    private static string Timestamp =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"))
            .ToString("dd/MM/yyyy HH:mm");

    public PetsController(IPetService petService)
    {
        _petService = petService;
    }

    // ─────────────────────────────────────────────
    // GET api/pets  (OData: $filter, $select, $orderby, $top, $skip, $count)
    // ─────────────────────────────────────────────
    /// <summary>Get all pets (OData enabled).</summary>
    [HttpGet]
    [EnableQuery(MaxTop = 100)]
    public ActionResult<IQueryable<Pet>> GetAll()
    {
        return Ok(_petService.GetAllQueryable());
    }

    // ─────────────────────────────────────────────
    // GET api/pets/{id}
    // ─────────────────────────────────────────────
    /// <summary>Get pet details by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var dto = await _petService.GetByIdAsync(id);
            return Ok(new { success = true, message = "Success.", data = dto, errors = (object?)null, timestamp = Timestamp });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message, data = (object?)null, errors = new[] { ex.Message }, timestamp = Timestamp });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Internal server error.", data = (object?)null, errors = new[] { ex.Message }, timestamp = Timestamp });
        }
    }

    // ─────────────────────────────────────────────
    // GET api/pets/customer/{customerId}
    // ─────────────────────────────────────────────
    /// <summary>Get list of pets by CustomerID.</summary>
    [HttpGet("customer/{customerId:guid}")]
    public async Task<IActionResult> GetByCustomer(Guid customerId)
    {
        try
        {
            var list = await _petService.GetByCustomerIdAsync(customerId);
            return Ok(new { success = true, message = "Success.", data = list, errors = (object?)null, timestamp = Timestamp });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Internal server error.", data = (object?)null, errors = new[] { ex.Message }, timestamp = Timestamp });
        }
    }

    // ─────────────────────────────────────────────
    // GET api/pets/search?customerId=&keyword=
    // ─────────────────────────────────────────────
    /// <summary>Search for pets by species or breed.</summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] Guid customerId, [FromQuery] string keyword)
    {
        try
        {
            var list = await _petService.SearchAsync(customerId, keyword);
            return Ok(new { success = true, message = "Success.", data = list, errors = (object?)null, timestamp = Timestamp });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message, data = (object?)null, errors = new[] { ex.Message }, timestamp = Timestamp });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Internal server error.", data = (object?)null, errors = new[] { ex.Message }, timestamp = Timestamp });
        }
    }

    // ─────────────────────────────────────────────
    // POST api/pets
    // ─────────────────────────────────────────────
    /// <summary>Add a new pet.</summary>
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] PetCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, message = "Invalid data.", data = (object?)null, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage), timestamp = Timestamp });

        try
        {
            var created = await _petService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.PetId },
                new { success = true, message = "Pet added successfully.", data = created, errors = (object?)null, timestamp = Timestamp });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message, data = (object?)null, errors = new[] { ex.Message }, timestamp = Timestamp });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Internal server error.", data = (object?)null, errors = new[] { ex.Message }, timestamp = Timestamp });
        }
    }

    // ─────────────────────────────────────────────
    // PUT api/pets/{id}
    // ─────────────────────────────────────────────
    /// <summary>Update pet information.</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] PetUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, message = "Invalid data.", data = (object?)null, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage), timestamp = Timestamp });

        try
        {
            var updated = await _petService.UpdateAsync(id, dto);
            return Ok(new { success = true, message = "Updated successfully.", data = updated, errors = (object?)null, timestamp = Timestamp });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message, data = (object?)null, errors = new[] { ex.Message }, timestamp = Timestamp });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Internal server error.", data = (object?)null, errors = new[] { ex.Message }, timestamp = Timestamp });
        }
    }

    // ─────────────────────────────────────────────
    // DELETE api/pets/{id}  (Soft Delete)
    // ─────────────────────────────────────────────
    /// <summary>Soft delete pet (set IsActive = false).</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _petService.SoftDeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message, data = (object?)null, errors = new[] { ex.Message }, timestamp = Timestamp });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Internal server error.", data = (object?)null, errors = new[] { ex.Message }, timestamp = Timestamp });
        }
    }
}