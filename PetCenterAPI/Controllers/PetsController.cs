using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCenterAPI.Service.Interface;
using System.Security.Claims;

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

        private Guid GetCustomerId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("my-pets")]
        public async Task<IActionResult> GetMyPets()
        {
            return Ok(await _petService.GetMyPetsAsync(GetCustomerId()));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetPetDetails(Guid id)
        {
            var pet = await _petService.GetPetDetailsAsync(id, GetCustomerId());
            if (pet == null) return NotFound(new { success = false, message = "Pet not found." });
            return Ok(pet);
        }
    }
}