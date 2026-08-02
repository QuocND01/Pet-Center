using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using PetCenterAPI.Service.Interface;
using static PetCenterAPI.DTOs.Requests.Pet.PetRequestDTO;

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

        // SỬA LỖI 1: Tận dụng [EnableQuery] của OData thay vì tự handle ODataQueryOptions.
        // PageSize = 50 đảm bảo dù client không truyền $top, server cũng chỉ trả tối đa 50 record, không bao giờ tràn RAM.
        [HttpGet]
        [EnableQuery(PageSize = 50, MaxTop = 100)]
        public IActionResult GetAllPets()
        {
            try
            {
                // Chỉ gọi Query, KHÔNG DÙNG .ToListAsync() ở đây.
                // OData Middleware sẽ tự động hứng IQueryable này, chèn thêm các câu lệnh SQL tương ứng
                // với $filter, $orderby, $skip, $top... rồi mới thực thi dưới Database.
                var baseQuery = _petService.GetAllPetsForVetQuery();

                return Ok(baseQuery);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("debug")]
        public async Task<IActionResult> Debug()
        {
            // SỬA LỖI 2: Đẩy .Take(10) lên trước .ToListAsync()
            // Đếm tổng số lượng record (nếu cần)
            var totalCount = await _petService.GetAllPetsForVetQuery().CountAsync();

            // Lấy 10 phần tử dưới DB đưa lên RAM
            var sample = await _petService.GetAllPetsForVetQuery()
                .Take(10)
                .ToListAsync();

            return Ok(new { count = totalCount, sample });
        }

        [HttpGet("raw-debug")]
        public async Task<IActionResult> RawDebug()
        {
            // SỬA LỖI 3: Giới hạn số lượng (Take 50) trước khi ToListAsync()
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
                })
                .Take(50) // Chặn đứng việc load toàn bộ DB
                .ToListAsync();

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