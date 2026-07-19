using PetCenterClient.Services.Interface;
using PetCenterClient.ViewModels;
using PetCenterClient.ViewModels.Appointment;
using System.Net.Http.Headers;

namespace PetCenterClient.Services
{
    public class AppointmentApiService : IAppointmentApiService
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AppointmentApiService(
            HttpClient http,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _http.BaseAddress = new Uri(configuration["Api:Url"]!);
            _httpContextAccessor = httpContextAccessor;
        }
        // Hàm helper để gán Token vào header cho mọi request
        private void AddAuthorizationHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT");

            if (!string.IsNullOrEmpty(token))
            {
                // Xóa các giá trị cũ để tránh cộng dồn header
                _http.DefaultRequestHeaders.Authorization = null;
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }
        public async Task<AppointmentBookingViewModel?> BookAppointmentAsync(BookAppointmentViewModel request)
        {
            AddAuthorizationHeader();
            // 1. Gửi request POST sang API
            var response = await _http.PostAsJsonAsync("api/Appointment/book", request);

            // 2. Nếu API báo lỗi (Mã trạng thái không phải 2xx, ví dụ: 400, 500)
            if (!response.IsSuccessStatusCode)
            {
                // Đọc nội dung lỗi thô dạng chuỗi JSON từ Server trả về
                var errorContent = await response.Content.ReadAsStringAsync();

                // IN LOG RA CONSOLE VỚI MÀU VÀNG/ĐỎ ĐỂ DỄ NHÌN
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("============= [API BOOKING ERROR] =============");
                Console.WriteLine($"Status Code: {(int)response.StatusCode} {response.StatusCode}");
                Console.WriteLine($"Chi tiết lỗi từ Server:\n{errorContent}");
                Console.WriteLine("===============================================");
                Console.ResetColor();

                // Thử parse ra ApiResponseViewModel để lấy message sạch nếu có
                try
                {
                    var errorResponse = System.Text.Json.JsonSerializer
                        .Deserialize<ApiResponseViewModel<object>>(errorContent, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (errorResponse != null && !string.IsNullOrEmpty(errorResponse.Message))
                    {
                        throw new Exception(errorResponse.Message);
                    }
                }
                catch (System.Text.Json.JsonException)
                {
                    // Nếu không phải định dạng ApiResponseViewModel chuẩn, ném luôn chuỗi error thô
                }

                throw new Exception($"Đã xảy ra lỗi khi đặt lịch: {response.StatusCode}");
            }

            // 3. Nếu thành công (200 OK / 201 Created) -> Đọc dữ liệu bình thường
            var apiResponse = await response.Content
                .ReadFromJsonAsync<ApiResponseViewModel<AppointmentBookingViewModel>>();

            if (apiResponse == null)
                throw new Exception("Cannot read response.");

            return apiResponse.Data;
        }

        public async Task<BookingPageViewModel?> GetBookingDataAsync()
        {
            AddAuthorizationHeader();
            var response = await _http.GetAsync(
                "api/Appointment/get-booking-data");

            var result = await response.Content
                .ReadFromJsonAsync<ApiResponseViewModel<BookingPageViewModel>>();

            if (result == null)
                throw new Exception("Cannot load booking data.");

            if (!response.IsSuccessStatusCode)
                throw new Exception(result.Message);

            return result.Data;
        }
        public async Task<List<AvailableSlotViewModel>> GetAvailableSlotsAsync(
    Guid staffId,
    DateOnly date,
    List<Guid> serviceIds)
        {
            AddAuthorizationHeader();

            var request = new
            {
                StaffId = staffId,
                Date = date,
                ServiceIds = serviceIds
            };

            var response = await _http.PostAsJsonAsync(
                "api/Appointment/available-slots",
                request);

            var result = await response.Content
                .ReadFromJsonAsync<ApiResponseViewModel<List<AvailableSlotViewModel>>>();

            if (result == null)
                throw new Exception("Cannot load available slots.");

            if (!response.IsSuccessStatusCode)
                throw new Exception(result.Message);

            return result.Data ?? [];
        }
        public async Task<List<AppointmentListViewModel>> GetMyAppointmentsAsync()
        {
            AddAuthorizationHeader();

            var response = await _http.GetAsync("api/Appointment/my");

            var result = await response.Content.ReadFromJsonAsync<
                ApiResponseViewModel<List<AppointmentListViewModel>>>();

            if (result == null)
                throw new Exception("Cannot load appointments.");

            if (!response.IsSuccessStatusCode)
                throw new Exception(result.Message);

            return result.Data ?? [];
        }
        public async Task<AppointmentDetailViewModel?> GetAppointmentDetailAsync(Guid appointmentId)
        {
            AddAuthorizationHeader();

            var response =
                await _http.GetAsync($"api/Appointment/{appointmentId}");

            var result = await response.Content.ReadFromJsonAsync<
                ApiResponseViewModel<AppointmentDetailViewModel>>();

            if (result == null)
                throw new Exception("Cannot load appointment.");

            if (!response.IsSuccessStatusCode)
                throw new Exception(result.Message);

            return result.Data;
        }
        public async Task CancelAppointmentAsync(Guid appointmentId)
        {
            AddAuthorizationHeader();

            var response =
                await _http.PutAsync(
                    $"api/Appointment/{appointmentId}/cancel",
                    null);

            var result =
                await response.Content.ReadFromJsonAsync<ApiResponseViewModel<object>>();

            if (result == null)
                throw new Exception("Cancel failed.");

            if (!response.IsSuccessStatusCode)
                throw new Exception(result.Message);
        }
        public async Task SubmitReviewAsync(
    SubmitReviewViewModel model)
        {
            AddAuthorizationHeader();

            var response =
                await _http.PutAsJsonAsync(
                    "api/Appointment/review",
                    model);

            var result =
                await response.Content.ReadFromJsonAsync<ApiResponseViewModel<object>>();

            if (result == null)
                throw new Exception("Submit review failed.");

            if (!response.IsSuccessStatusCode)
                throw new Exception(result.Message);
        }
    }
}