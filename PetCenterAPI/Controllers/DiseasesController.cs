using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using PetCenterAPI.Service.Interface;
using static PetCenterAPI.DTOs.Requests.DiseaseDTO;

namespace PetCenterAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Vet,Admin")] // Chỉ định quyền cho Vet/Admin
    public class DiseasesController : ControllerBase
    {
        private readonly IDiseaseService _diseaseService;

        public DiseasesController(IDiseaseService diseaseService)
        {
            _diseaseService = diseaseService;
        }

        // GET: api/Diseases
        // Hỗ trợ OData: api/Diseases?$filter=contains(Name, 'Fever')&$orderby=CreatedAt desc
        [HttpGet]
        [EnableQuery]
        public IActionResult GetAllDiseases()
        {
            var query = _diseaseService.GetAllDiseasesQuery();
            return Ok(query);
        }

        // GET: api/Diseases/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetDiseaseDetails(Guid id)
        {
            var disease = await _diseaseService.GetDiseaseByIdAsync(id);
            if (disease == null) return NotFound(new { success = false, message = "Disease not found." });
            return Ok(disease);
        }

        // POST: api/Diseases
        [HttpPost]
        public async Task<IActionResult> AddDisease([FromBody] MutateDiseaseDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var success = await _diseaseService.AddDiseaseAsync(dto);
            return success ? Ok(new { success = true, message = "Disease added successfully." }) : BadRequest(new { success = false, message = "Failed to add disease." });
        }

        // PUT: api/Diseases/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateDisease(Guid id, [FromBody] MutateDiseaseDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var success = await _diseaseService.UpdateDiseaseAsync(id, dto);
            return success ? Ok(new { success = true, message = "Disease updated successfully." }) : BadRequest(new { success = false, message = "Disease not found." });
        }

        // DELETE: api/Diseases/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteDisease(Guid id)
        {
            try
            {
                var success = await _diseaseService.DeleteDiseaseAsync(id);
                return success ? Ok(new { success = true, message = "Disease deleted successfully." }) : BadRequest(new { success = false, message = "Disease not found." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message }); // Bắt lỗi không cho xóa IsSystem
            }
        }
    }
}