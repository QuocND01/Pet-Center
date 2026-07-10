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
        public async Task<IActionResult> GetDashboardMetrics()
        {
            // Controller cực kỳ sạch sẽ, mọi logic tính toán đã được đẩy xuống Service
            var metrics = await _analyticsService.GetDashboardDataAsync();
            return Ok(metrics);
        }
    }
}