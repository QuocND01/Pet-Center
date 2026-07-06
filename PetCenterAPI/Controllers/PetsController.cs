using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using PetCenterAPI.Service.Interface;
using System.Security.Claims;
using static PetCenterAPI.DTOs.Requests.CustomerProfile.PetRequestDTO;

namespace PetCenterAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Customer")]
    public class PetsController : ControllerBase
    {
        private readonly IPetService _petService;

        public PetsController(IPetService petService)
        {
            _petService = petService;
        }

        // Lấy CustomerId từ JWT Token
        private Guid GetCustomerId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("my-pets")]
        [EnableQuery] // Bật OData cho danh sách
        public IActionResult GetMyPets()
        {
            var query = _petService.GetMyPetsQuery(GetCustomerId());
            return Ok(query);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetPetDetails(Guid id)
        {
            var pet = await _petService.GetPetDetailsAsync(id, GetCustomerId());
            if (pet == null) return NotFound(new { success = false, message = "Pet not found." });
            return Ok(pet);
        }

        [HttpPost]
        public async Task<IActionResult> AddPet([FromBody] MutatePetDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var success = await _petService.AddPetAsync(GetCustomerId(), dto);
            return success ? Ok(new { success = true, message = "Pet added successfully." }) : BadRequest(new { success = false, message = "Failed to add pet." });
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdatePet(Guid id, [FromBody] MutatePetDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var success = await _petService.UpdatePetAsync(id, GetCustomerId(), dto, false);
            return success ? Ok(new { success = true, message = "Pet updated successfully." }) : BadRequest(new { success = false, message = "Pet not found or update failed." });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeletePet(Guid id)
        {
            var success = await _petService.DeletePetAsync(id, GetCustomerId(), false);
            return success ? Ok(new { success = true, message = "Pet deleted successfully." }) : BadRequest(new { success = false, message = "Pet not found or delete failed." });
        }
    }
}