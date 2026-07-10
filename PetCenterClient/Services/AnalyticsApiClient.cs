using PetCenterClient.Services.Interface;
using PetCenterClient.ViewModels;
using System.Net.Http.Headers;
using System.Net.Http.Json;

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

        public async Task<DashboardMetricsViewModel> GetDashboardMetricsAsync(string? startDate = null, string? endDate = null)
        {
            // 1. Lấy JWT Token từ Session của người dùng đăng nhập
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT");
            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // 2. Cấu hình URL gọi sang Backend API
            var url = "https://localhost:7004/api/analytics/dashboard";

            // Nếu có truyền tham số lọc ngày thì ghép chuỗi Query String vào URL
            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                url += $"?startDate={startDate}&endDate={endDate}";
            }

            try
            {
                var response = await _http.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<DashboardMetricsViewModel>()
                           ?? new DashboardMetricsViewModel();
                }
            }
            catch (Exception ex)
            {
                // Log lỗi nếu kết nối API thất bại
                Console.WriteLine($"Analytics API Error: {ex.Message}");
            }

            // Trả về object trống kèm các danh sách khởi tạo sẵn để tránh lỗi NullReferenceException ngoài View
            return new DashboardMetricsViewModel();
        }
    }
}