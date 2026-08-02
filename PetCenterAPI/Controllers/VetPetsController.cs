using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using PetCenterAPI.Service.Interface;
using static PetCenterAPI.DTOs.Requests.CustomerProfile.PetRequestDTO;

namespace PetCenterAPI.Controllers
{
    [Route("api/vet/pets")]
    [ApiController]
    [Authorize(Roles = "Vet,Admin,Groomer")]
    public class VetPetsController : ControllerBase
    {
        private readonly IPetService _petService;

        public VetPetsController(IPetService petService)
        {
            _petService = petService;
        }

        [HttpGet]
        //public async Task<IActionResult> GetAllPets([FromQuery] ODataQueryOptions<PetCenterAPI.DTOs.Requests.VetPetRequestDTO.ReadVetPetListDTO> queryOptions)
        //{
        //    // Support OData $filter/$orderby/$skip/$top from client (the client builds OData query string)
        //    try
        //    {
        //        var baseQuery = _petService.GetAllPetsForVetQuery();

        //        if (queryOptions != null && (queryOptions.Filter != null || queryOptions.OrderBy != null || queryOptions.Skip != null || queryOptions.Top != null))
        //        {
        //            var filtered = (IQueryable<PetCenterAPI.DTOs.Requests.VetPetRequestDTO.ReadVetPetListDTO>)queryOptions.ApplyTo(baseQuery);
        //            var list = await filtered.ToListAsync();
        //            return Ok(list);
        //        }

        //        var listAll = await baseQuery.ToListAsync();
        //        return Ok(listAll);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { success = false, message = ex.Message });
        //    }
        //}

        [HttpGet("debug")]
        public async Task<IActionResult> Debug()
        {
            // Diagnostic endpoint: returns count and a small sample of pet records
            var list = await _petService.GetAllPetsForVetQuery().ToListAsync();
            var sample = list.Take(10);
            return Ok(new { count = list.Count, sample });
        }

        [HttpGet("raw-debug")]
        public async Task<IActionResult> RawDebug()
        {
            // Return raw Pet rows from DB for inspection (PetId, PetName, Breed, Species, CustomerId, IsActive)
            var raw = await _petService.GetAllPetsForVetQuery()
                .Select(p => new {
                    p.PetId,
                    p.PetName,
                    p.Breed,
                    p.Species,
                    p.DateOfBirth,
                    p.PetAvatar,
                    p.OwnerName,
                    p.OwnerPhone
                }).ToListAsync();
            return Ok(new { count = raw.Count, raw });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetPetDetails(Guid id)
        {
            var pet = await _petService.GetPetDetailForVetAsync(id);
            if (pet == null) return NotFound(new { success = false, message = "Pet not found." });
            return Ok(pet);
        }

        [HttpPost("add-for-customer/{customerId:guid}")]
        public async Task<IActionResult> AddPetForCustomer(Guid customerId, [FromForm] MutatePetDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var success = await _petService.AddPetAsync(customerId, dto);
            return success ? Ok(new { success = true, message = "Pet added successfully." }) : BadRequest(new { success = false, message = "Failed to add pet." });
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdatePet(Guid id, [FromForm] MutatePetDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            // Truyền Guid.Empty cho CustomerId vì isVet = true (bỏ qua check chính chủ)
            var success = await _petService.UpdatePetAsync(id, Guid.Empty, dto, true);
            return success ? Ok(new { success = true, message = "Pet updated successfully." }) : BadRequest(new { success = false, message = "Pet not found or update failed." });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeletePet(Guid id)
        {
            // Truyền Guid.Empty cho CustomerId vì isVet = true (bỏ qua check chính chủ)
            var success = await _petService.DeletePetAsync(id, Guid.Empty, true);
            return success ? Ok(new { success = true, message = "Pet deleted successfully." }) : BadRequest(new { success = false, message = "Pet not found or delete failed." });
        }
    }
}