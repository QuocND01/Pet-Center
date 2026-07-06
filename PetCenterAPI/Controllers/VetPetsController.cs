using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCenterAPI.Service.Interface;

namespace PetCenterAPI.Controllers
{
    [Route("api/vet/pets")]
    [ApiController]
    [Authorize(Roles = "Vet,Admin")] // Chỉ Vet hoặc Admin mới được xem
    public class VetPetsController : ControllerBase
    {
        private readonly IPetService _petService;

        public VetPetsController(IPetService petService)
        {
            _petService = petService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPets()
        {
            return Ok(await _petService.GetAllPetsForVetAsync());
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetPetDetails(Guid id)
        {
            var pet = await _petService.GetPetDetailForVetAsync(id);
            if (pet == null) return NotFound(new { success = false, message = "Pet not found." });
            return Ok(pet);
        }
    }
}