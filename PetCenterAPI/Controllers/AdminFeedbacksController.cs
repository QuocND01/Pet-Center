using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCenterAPI.DTOs.Requests.ManageFeedback;
using PetCenterAPI.DTOs.Responses.ManageFeedback;
using PetCenterAPI.Service.Interface;

namespace PetCenterAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,SaleStaff")]
    public class AdminFeedbacksController : ControllerBase
    {
        private readonly IAdminFeedbackService _adminFeedbackService;

        public AdminFeedbacksController(IAdminFeedbackService adminFeedbackService)
        {
            _adminFeedbackService = adminFeedbackService;
        }

        // ============================================================
        // FEEDBACK — VIEW LIST (ADMIN/STAFF)
        // ============================================================
        [HttpGet("list")]
        public async Task<IActionResult> GetAll([FromQuery] FeedbackFilterRequestDTO filter)
        {
            try
            {
                var result = await _adminFeedbackService.GetAllAsync(filter);
                if (!result.Success) return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Fail(ex.Message));
            }
        }
    }
}
