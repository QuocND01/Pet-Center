using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using PetCenterAPI.Service.Interface;
using static PetCenterAPI.DTOs.Requests.Disease.DiseaseDTO;

namespace PetCenterAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Vet,Admin,Groomer")]
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
            // Sanitize inputs: trim and collapse consecutive spaces before validation
            SanitizeDiseaseDto(dto);

            // Re-validate model after sanitization
            if (!TryValidateModel(dto)) return BadRequest(ModelState);

            // Kiểm tra trùng tên trước khi gọi service để trả message rõ ràng
            var name = dto.Name;
            if (!string.IsNullOrEmpty(name))
            {
                var exists = _diseaseService.GetAllDiseasesQuery()
                    .Any(d => d.Name.ToLower() == name.ToLower());
                if (exists) return BadRequest(new { success = false, message = "Disease name already exists." });
            }

            var success = await _diseaseService.AddDiseaseAsync(dto);
            return success ? Ok(new { success = true, message = "Disease added successfully." }) : BadRequest(new { success = false, message = "Failed to add disease." });
        }

        // PUT: api/Diseases/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateDisease(Guid id, [FromBody] MutateDiseaseDTO dto)
        {
            // Sanitize inputs before validation
            SanitizeDiseaseDto(dto);

            if (!TryValidateModel(dto)) return BadRequest(ModelState);

            var success = await _disease_service_stub(id, dto);
            return success ? Ok(new { success = true, message = "Disease updated successfully." }) : BadRequest(new { success = false, message = "Disease not found." });
        }

        // wrapper to call service (kept for clearer DI in unit tests if needed)
        private Task<bool> _disease_service_stub(Guid id, MutateDiseaseDTO dto)
        {
            return _diseaseService.UpdateDiseaseAsync(id, dto);
        }

        // Helper: trim and collapse multiple consecutive spaces inside strings
        private static void SanitizeDiseaseDto(MutateDiseaseDTO dto)
        {
            if (dto == null) return;
            dto.Name = CollapseSpaces(dto.Name);
            dto.Description = CollapseSpaces(dto.Description);
            dto.Recommendation = CollapseSpaces(dto.Recommendation);
        }

        private static string? CollapseSpaces(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input?.Trim();
            // Replace multiple whitespace characters with a single space, then trim
            var collapsed = System.Text.RegularExpressions.Regex.Replace(input, "\\s+", " ");
            return collapsed.Trim();
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