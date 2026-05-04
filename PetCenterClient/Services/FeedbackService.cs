using Newtonsoft.Json;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;
using System.Net.Http.Headers;
using System.Text;

namespace PetCenterClient.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FeedbackService(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        private void AttachToken()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT") ?? "";
            _http.DefaultRequestHeaders.Authorization = null;
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<bool> HasFeedbackForOrderAsync(Guid orderId)
        {
            try
            {
                AttachToken();
                var response = await _http.GetAsync(
                    $"feedback-service/ProductFeedback/check/{orderId}");

                if (!response.IsSuccessStatusCode) return false;

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<dynamic>(content);
                return (bool)(result?.hasFeedback ?? false);
            }
            catch { return false; }
        }

        public async Task<List<ProductFeedbackResponseDto>> GetFeedbacksByOrderIdAsync(Guid orderId)
        {
            try
            {
                AttachToken();
                var response = await _http.GetAsync(
                    $"feedback-service/ProductFeedback/order/{orderId}");

                if (!response.IsSuccessStatusCode) return new List<ProductFeedbackResponseDto>();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponse<List<ProductFeedbackResponseDto>>>(content);
                return result?.Data ?? new List<ProductFeedbackResponseDto>();
            }
            catch { return new List<ProductFeedbackResponseDto>(); }
        }

        public async Task<bool> CreateBulkFeedbackAsync(CreateBulkFeedbackDto dto)
        {
            try
            {
                AttachToken();
                var json = JsonConvert.SerializeObject(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _http.PostAsync(
                    "feedback-service/ProductFeedback/bulk", content);

                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<bool> UpdateFeedbackAsync(UpdateProductFeedbackDto dto)
        {
            try
            {
                AttachToken();
                var json = JsonConvert.SerializeObject(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _http.PutAsync(
                    "feedback-service/ProductFeedback/update", content);

                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<List<ProductFeedbackResponseDto>> GetFeedbacksByProductIdAsync(Guid productId)
        {
            try
            {
                var response = await _http.GetAsync(
                    $"feedback-service/ProductFeedback/product/{productId}");

                if (!response.IsSuccessStatusCode)
                    return new List<ProductFeedbackResponseDto>();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponse<List<ProductFeedbackResponseDto>>>(content);
                return result?.Data ?? new List<ProductFeedbackResponseDto>();
            }
            catch
            {
                return new List<ProductFeedbackResponseDto>();
            }
        }
    }


}