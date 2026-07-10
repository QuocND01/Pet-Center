using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCenterAPI.Services.Interfaces;

namespace PetCenterAPI.Controllers
{
    [Route("api/analytics")]
    [ApiController]
    [Authorize(Roles = "Admin,Vet,Sale Staff")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardMetrics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var metrics = await _analyticsService.GetDashboardDataAsync(startDate, endDate);
            return Ok(metrics);
        }
    }
}