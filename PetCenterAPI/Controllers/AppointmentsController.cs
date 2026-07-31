using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PetCenterAPI.DTOs.Requests.Appointment;
using PetCenterAPI.Service.Interface;
using System.Security.Claims;
using System.Text.Json;

namespace PetCenterAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpPost("book")]
        public async Task<IActionResult> BookAppointment(BookAppointmentRequestDTO request)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claim))
            {
                return Unauthorized(new { status = false, message = "CustomerId missing in token" });
            }

            request.CustomerId = Guid.Parse(claim);
            try
            {
                var result = await _appointmentService.BookAppointmentAsync(request);
                return Ok(new { status = true, message = "Book appointment successfully.", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = false, message = ex.Message });
            }
        }

        [HttpGet("get-booking-data")]
        public async Task<IActionResult> GetBookingData()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claim))
            {
                return Unauthorized(new { status = false, message = "CustomerId missing in token" });
            }

            try
            {
                var result = await _appointmentService.GetBookingDataAsync(Guid.Parse(claim));
                return Ok(new { status = true, message = "Get booking data successfully.", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = false, message = ex.Message });
            }
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyAppointments()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claim))
            {
                return Unauthorized(new { status = false, message = "Customer must login to view appointments" });
            }

            try
            {
                var result = await _appointmentService.GetMyAppointmentsAsync(Guid.Parse(claim));
                return Ok(new { status = true, message = "Get appointments successfully.", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = false, message = ex.Message });
            }
        }

        [HttpGet("{appointmentId}")]
        public async Task<IActionResult> GetAppointmentDetail(Guid appointmentId)
        {
            try
            {
                var result = await _appointmentService.GetAppointmentDetailAsync(appointmentId);
                return Ok(new { status = true, message = "Get appointment successfully.", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = false, message = ex.Message });
            }
        }

        [HttpPut("{appointmentId}/cancel")]
        public async Task<IActionResult> CancelAppointment(Guid appointmentId)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claim))
            {
                return Unauthorized(new { status = false, message = "CustomerId missing in token" });
            }

            try
            {
                await _appointmentService.CancelAppointmentAsync(appointmentId, Guid.Parse(claim));
                return Ok(new { status = true, message = "Appointment cancelled successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = false, message = ex.Message });
            }
        }

        [HttpPut("review")]
        public async Task<IActionResult> SubmitReview(SubmitReviewRequestDTO request)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claim))
            {
                return Unauthorized(new { status = false, message = "CustomerId missing in token" });
            }

            try
            {
                await _appointmentService.SubmitReviewAsync(Guid.Parse(claim), request);
                return Ok(new { status = true, message = "Review submitted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = false, message = ex.Message });
            }
        }

        [HttpPost("available-slots")]
        public async Task<IActionResult> GetAvailableSlots([FromBody] GetAvailableSlotsRequestDTO request)
        {
            try
            {
                var result = await _appointmentService.GetAvailableSlotsAsync(request);
                return Ok(new { status = true, message = "Get available slots successfully.", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = false, message = ex.Message });
            }
        }

        [HttpPut("{appointmentId}/forward")]
        public async Task<IActionResult> ForwardAppointmentStatus(Guid appointmentId)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claim))
            {
                return Unauthorized(new { status = false, message = "Staff Id missing in token" });
            }

            try
            {
                await _appointmentService.ForwardAppointmentStatusAsync(appointmentId, Guid.Parse(claim));
                return Ok(new { status = true, message = "Appointment status forwarded successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = false, message = ex.Message });
            }
        }

        [HttpPut("{appointmentserviceId}/complete")]
        public async Task<IActionResult> CompleteAppointmentService(Guid appointmentserviceId)
        {
            try
            {
                await _appointmentService.CompleteAppointmentService(appointmentserviceId);
                return Ok(new { status = true, message = "Appointment service completed successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = false, message = ex.Message });
            }
        }

        [HttpGet("appointments")]
        public async Task<IActionResult> GetAllAppointments()
        {
            try
            {
                var result = await _appointmentService.GetAllAppointmentsAsync();
                return Ok(new { status = true, message = "Get appointments successfully.", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Tạo link thanh toán cho Appointment đang ở trạng thái Reserved (1)
        /// Route: POST api/Appointment/payment/create
        /// </summary>
        [HttpPost("payment/create")]
        public async Task<IActionResult> CreatePaymentUrl([FromBody] AppointmentPaymentRequestDTO request)
        {
            if (string.IsNullOrEmpty(request.ClientIpAddress))
            {
                request.ClientIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            }

            try
            {
                var result = await _appointmentService.CreatePaymentUrlAsync(request);
                return Ok(new { status = true, message = "Khởi tạo thanh toán thành công.", data = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { status = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { status = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = false, message = "Lỗi xử lý hệ thống.", detail = ex.Message });
            }
        }

        /// <summary>
        /// Endpoint nhận callback/return từ VNPAY
        /// Route: GET api/Appointment/vnpay-callback
        /// </summary>
        [HttpGet("vnpay-callback")]
        [AllowAnonymous]
        public async Task<IActionResult> VnPayCallback()
        {
            var result = await _appointmentService.ProcessVnPayCallbackAsync(Request.Query);
            if (result.IsSuccess)
            {
                return Ok(new { status = true, message = result.Message, data = result });
            }
            return BadRequest(new { status = false, message = result.Message, data = result });
        }

        /// <summary>
        /// Endpoint nhận callback/IPN từ MoMo
        /// Route: POST api/Appointment/momo-callback
        /// </summary>
        [HttpPost("momo-callback")]
        [AllowAnonymous]
        public async Task<IActionResult> MoMoCallback([FromBody] JsonElement body)
        {
            string signature = Request.Headers["X-MoMo-Signature"].ToString();
            string rawBody = body.GetRawText();

            var result = await _appointmentService.ProcessMoMoCallbackAsync(body, rawBody, signature);
            if (result.IsSuccess)
            {
                return Ok(new { status = true, message = result.Message, data = result });
            }
            return BadRequest(new { status = false, message = result.Message, data = result });
        }
        /// <summary>
        /// Cập nhật lại thông tin dịch vụ, nhân viên, thời gian cho lịch hẹn đang giữ chỗ (Status = 1)
        /// Route: PUT api/Appointment/update-reserved
        /// </summary>
        [HttpPut("update-reserved")]
        public async Task<IActionResult> UpdateReservedAppointment([FromBody] UpdateAppointmentRequestDTO request)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claim))
            {
                return Unauthorized(new { status = false, message = "CustomerId missing in token" });
            }

            try
            {
                var result = await _appointmentService.UpdateReservedAppointmentAsync(request, Guid.Parse(claim));
                return Ok(new { status = true, message = "Update reserved appointment successfully.", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = false, message = ex.Message });
            }
        }
    }
}