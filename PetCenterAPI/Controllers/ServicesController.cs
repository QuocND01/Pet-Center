using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using PetCenterAPI.Common;
using PetCenterAPI.Service.Interface;
using static PetCenterAPI.DTOs.Requests.Service.ServiceRequestDTO;
using static PetCenterAPI.DTOs.Responses.Service.ServiceResponseDTO;

namespace PetCenterAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly IServiceService _ServiceService;

        public ServicesController(IServiceService ServiceService)
        {
            _ServiceService = ServiceService;
        }

        // GET: api/
        [HttpGet]
        public async Task<IActionResult> Get(ODataQueryOptions<ReadServiceDTOForCustomer> queryOptions)
        {
            var result = await _ServiceService.GetAllServiceAsync(queryOptions);
            return Ok(result);
        }


        [HttpGet("admin")]
        public async Task<ActionResult<PagedResult<ReadServiceDTO>>> GetAllServiceAdminAsync(
    [FromQuery] ServiceSpecification spec)
        {
            var result = await _ServiceService.GetAllServiceAdminAsync(spec);
            return Ok(result);
        }

        // GET: api/Services/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReadServiceDTO>> DetailsServiceAsync(Guid id)
        {
            var Service = await _ServiceService.GetServiceByIdAsync(id);

            if (Service == null)
            {
                return NotFound();
            }

            return Service;
        }

        // PUT: api/Services/5
        // [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutServiceAsync(
            Guid id,
            [FromForm] UpdateServiceDTO Service)
        {

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new { message = string.Join(", ", errors) });
            }

            try
            {
                await _ServiceService.UpdateServiceAsync(id, Service);

                return Ok(new { success = true, message = "Service updated successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                return StatusCode(500, new
                {
                    success = false,
                    message = ex.ToString()
                });
            }
        }

        // POST: api/Services
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> PostServiceAsync([FromForm] CreateServiceDTO Service)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new
                {
                    success = false,
                    message = string.Join(", ", errors)
                });
            }

            try
            {
                await _ServiceService.AddServiceAsync(Service);

                return Ok(new
                {
                    success = true,
                    message = "Service created successfully"
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // DELETE: api/Services/5
        // [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeStatus(
      Guid id,
      [FromBody] Status status)
        {
            Console.WriteLine(status);
            try
            {
                await _ServiceService.ChangeServiceStatusAsync(id, status);

                return Ok(new
                {
                    success = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("===== ERROR =====");
                Console.WriteLine(ex.ToString());

                return StatusCode(500, new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message,
                    stack = ex.StackTrace
                });
            }
        }
    }
}
