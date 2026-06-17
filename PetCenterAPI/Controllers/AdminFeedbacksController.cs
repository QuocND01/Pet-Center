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

        // ============================================================
        // FEEDBACK — VIEW DETAIL (ADMIN/STAFF)
        // ============================================================
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _adminFeedbackService.GetByIdAsync(id);
                if (!result.Success) return NotFound(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Fail(ex.Message));
            }
        }

        // ============================================================
        // FEEDBACK — REPLY
        // ============================================================
        [HttpPost("reply")]
        public async Task<IActionResult> Reply([FromBody] ReplyFeedbackRequestDTO request)
        {
            try
            {
                if (request.FeedbackId == Guid.Empty || request.StaffId == Guid.Empty)
                    return BadRequest(ApiResponse<bool>.Fail("FeedbackId and StaffId cannot be empty."));

                if (string.IsNullOrWhiteSpace(request.ReplyContent))
                    return BadRequest(ApiResponse<bool>.Fail("Reply content cannot be empty."));

                var result = await _adminFeedbackService.ReplyAsync(request);
                if (!result.Success) return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Fail(ex.Message));
            }
        }

        // ============================================================
        // FEEDBACK — UPDATE REPLY
        // ============================================================
        [HttpPut("reply")]
        public async Task<IActionResult> UpdateReply([FromBody] UpdateReplyRequestDTO request)
        {
            try
            {
                if (request.FeedbackId == Guid.Empty)
                    return BadRequest(ApiResponse<bool>.Fail("FeedbackId cannot be empty."));

                if (string.IsNullOrWhiteSpace(request.ReplyContent))
                    return BadRequest(ApiResponse<bool>.Fail("Reply content cannot be empty."));

                var result = await _adminFeedbackService.UpdateReplyAsync(request);
                if (!result.Success) return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Fail(ex.Message));
            }
        }

        // ============================================================
        // FEEDBACK — DELETE REPLY
        // ============================================================
        [HttpDelete("reply/{id:guid}")]
        public async Task<IActionResult> DeleteReply(Guid id)
        {
            try
            {
                var result = await _adminFeedbackService.DeleteReplyAsync(id);
                if (!result.Success) return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Fail(ex.Message));
            }
        }

        // ============================================================
        // FEEDBACK — TOGGLE VISIBILITY
        // ============================================================
        [HttpPatch("visibility")]
        public async Task<IActionResult> ToggleVisibility(
            [FromQuery] Guid feedbackId,
            [FromQuery] bool isVisible)
        {
            try
            {
                if (feedbackId == Guid.Empty)
                    return BadRequest(ApiResponse<bool>.Fail("FeedbackId cannot be empty."));

                var result = await _adminFeedbackService.ToggleVisibilityAsync(feedbackId, isVisible);
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
