using Microsoft.AspNetCore.Mvc;
using OrdersAPI.Service.Interface;

namespace OrdersAPI.Controllers
{
    [Route("api/[controller]")]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService _statsService;
        public StatisticsController(IStatisticsService statsService) => _statsService = statsService;

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromQuery] int? year)
        {
            int targetYear = year ?? DateTime.Now.Year;
            return Ok(await _statsService.GetAdminDashboardStatsAsync(targetYear));
        }
    }
}
