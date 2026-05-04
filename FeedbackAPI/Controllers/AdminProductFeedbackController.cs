using FeedbackAPI.DTOs;
using FeedbackAPI.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,SaleStaff")]   // ← chỉ Admin và SaleStaff
    public class AdminProductFeedbackController : ControllerBase
    {
        private readonly IAdminFeedbackService _service;

        public AdminProductFeedbackController(IAdminFeedbackService service)
        {
            _service = service;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetAll([FromQuery] FeedbackFilterDto filter)
        {
            try
            {
                var result = await _service.GetAllAsync(filter);
                if (!result.Success) return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Fail(ex.Message));
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _service.GetByIdAsync(id);
                if (!result.Success) return NotFound(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Fail(ex.Message));
            }
        }

        [HttpPost("reply")]
        public async Task<IActionResult> Reply([FromBody] ReplyFeedbackDto dto)
        {
            try
            {
                if (dto.FeedbackId == Guid.Empty || dto.StaffId == Guid.Empty)
                    return BadRequest(ApiResponse<bool>.Fail("FeedbackId và StaffId không được để trống."));

                if (string.IsNullOrWhiteSpace(dto.ReplyContent))
                    return BadRequest(ApiResponse<bool>.Fail("Nội dung reply không được để trống."));

                var result = await _service.ReplyAsync(dto);
                if (!result.Success) return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Fail(ex.Message));
            }
        }

        [HttpPut("reply")]
        public async Task<IActionResult> UpdateReply([FromBody] UpdateReplyDto dto)
        {
            try
            {
                if (dto.FeedbackId == Guid.Empty)
                    return BadRequest(ApiResponse<bool>.Fail("FeedbackId không được để trống."));

                if (string.IsNullOrWhiteSpace(dto.ReplyContent))
                    return BadRequest(ApiResponse<bool>.Fail("Nội dung reply không được để trống."));

                var result = await _service.UpdateReplyAsync(dto);
                if (!result.Success) return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Fail(ex.Message));
            }
        }

        [HttpDelete("reply/{id:guid}")]
        public async Task<IActionResult> DeleteReply(Guid id)
        {
            try
            {
                var result = await _service.DeleteReplyAsync(id);
                if (!result.Success) return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Fail(ex.Message));
            }
        }

        [HttpPatch("visibility")]
        public async Task<IActionResult> ToggleVisibility(
            [FromQuery] Guid feedbackId,
            [FromQuery] bool isVisible)
        {
            try
            {
                if (feedbackId == Guid.Empty)
                    return BadRequest(ApiResponse<bool>.Fail("FeedbackId không được để trống."));

                var result = await _service.ToggleVisibilityAsync(feedbackId, isVisible);
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
