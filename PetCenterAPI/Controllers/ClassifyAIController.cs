using Microsoft.AspNetCore.Mvc;
using PetCenterAPI.DTOs.Requests.AI;
using PetCenterAPI.Service.Interface;

namespace PetCenterAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly IClassifyAIService _aiService;

        public AIController(IClassifyAIService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost("predict")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(AIResultDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> Predict([FromForm] AIPredictRequest request)
        {
            if (request.File == null || request.File.Length == 0)
            {
                return BadRequest("Image is required.");
            }

            var result = await _aiService.PredictAsync(request.File);

            if (result == null)
            {
                return NotFound("Disease information not found.");
            }

            return Ok(result);
        }
    }
}
