using PetCenterClient.Services.Interface;
using PetCenterClient.ViewModels;
using System.Net.Http.Headers;

namespace PetCenterClient.Services
{
    public class AnalyticsApiClient : IAnalyticsApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AnalyticsApiClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<DashboardMetricsViewModel> GetDashboardMetricsAsync()
        {
            // Bóc JWT Token từ Session
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT");
            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _http.GetAsync("https://localhost:7004/api/analytics/dashboard");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<DashboardMetricsViewModel>()
                       ?? new DashboardMetricsViewModel();
            }

            return new DashboardMetricsViewModel(); // Trả về object rỗng nếu lỗi
        }
    }
}