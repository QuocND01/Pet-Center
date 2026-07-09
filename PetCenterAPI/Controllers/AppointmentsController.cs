using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PetCenterAPI.DTOs.Requests.Appointment;
using PetCenterAPI.Service.Interface;
using System.Security.Claims;

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
            var Claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(Claim))
            {
                return Unauthorized("CustomerId missing in token");
            }

            var id = Guid.Parse(Claim);
            request.CustomerId = id;
            try
            {
                var result = await _appointmentService.BookAppointmentAsync(request);

                return Ok(new
                {
                    status = true,
                    message = "Book appointment successfully.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }
        [HttpGet("get-booking-data")]
        public async Task<IActionResult> GetBookingData()
        {
            var Claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(Claim))
            {
                return Unauthorized("CustomerId missing in token");
            }

            var Id = Guid.Parse(Claim);


            try
            {
                var result = await _appointmentService.GetBookingDataAsync(Id);
                return Ok(new
                {
                    status = true,
                    message = "Get booking data successfully.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }
    }
}