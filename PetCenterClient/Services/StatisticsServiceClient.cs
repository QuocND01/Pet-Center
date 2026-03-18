using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;
using System.Net.Http.Headers;

namespace PetCenterClient.Services
{
    public class StatisticsServiceClient : IStatisticsServiceClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public StatisticsServiceClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<DashboardStatsDto?> GetDashboardStatsAsync(int? year = null)
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT");
            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            int targetYear = year ?? DateTime.Now.Year;
            return await _http.GetFromJsonAsync<DashboardStatsDto>($"gateway/Statistics/summary?year={targetYear}");
        }
    }
}