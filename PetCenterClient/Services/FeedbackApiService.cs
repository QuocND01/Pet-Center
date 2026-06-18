using Newtonsoft.Json;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;
using PetCenterClient.ViewModels.ManageFeedback;
using System.Net.Http.Headers;
using System.Text;

namespace PetCenterClient.Services
{
    public class FeedbackApiService : IFeedbackApiService
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FeedbackApiService(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        // ============================================================
        // HELPER
        // ============================================================
        private void AttachToken()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT") ?? "";
            _http.DefaultRequestHeaders.Authorization = null;
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        // ============================================================
        // FEEDBACK — VIEW BY ORDER (CUSTOMER SIDE — ORDER DETAIL POPUP)
        // ============================================================
        public async Task<bool> HasFeedbackForOrderAsync(Guid orderId)
        {
            try
            {
                AttachToken();

                var response = await _http.GetAsync($"api/ProductFeedbacks/check/{orderId}");
                if (!response.IsSuccessStatusCode) return false;

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<CheckFeedbackResponseViewModel>(content);

                return result?.HasFeedback ?? false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FeedbackApiService] HasFeedbackForOrderAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<List<ProductFeedbackViewModel>> GetFeedbacksByOrderIdAsync(Guid orderId)
        {
            try
            {
                AttachToken();
                var response = await _http.GetAsync(
                    $"api/ProductFeedbacks/order/{orderId}");

                if (!response.IsSuccessStatusCode) return new List<ProductFeedbackViewModel>();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponse<List<ProductFeedbackViewModel>>>(content);
                return result?.Data ?? new List<ProductFeedbackViewModel>();
            }
            catch { return new List<ProductFeedbackViewModel>(); }
        }

        public async Task<bool> CreateBulkFeedbackAsync(MultipartFormDataContent formData)
        {
            try
            {
                AttachToken();
                var response = await _http.PostAsync(
                   "feedback-service/ProductFeedback/bulk", formData);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<bool> UpdateFeedbackAsync(MultipartFormDataContent formData)
        {
            try
            {
                AttachToken();
                var response = await _http.PutAsync(
                    "feedback-service/ProductFeedback/update", formData);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        // ============================================================
        // FEEDBACK — VIEW BY PRODUCT (CUSTOMER SIDE)
        // ============================================================
        public async Task<List<ProductFeedbackViewModel>> GetFeedbacksByProductIdAsync(Guid productId)
        {
            try
            {
                var response = await _http.GetAsync(
                    $"api/ProductFeedbacks/product/{productId}");

                if (!response.IsSuccessStatusCode)
                    return new List<ProductFeedbackViewModel>();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<FeedbackApiResponseViewModel<List<ProductFeedbackViewModel>>>(content);
                return result?.Data ?? new List<ProductFeedbackViewModel>();
            }
            catch
            {
                return new List<ProductFeedbackViewModel>();
            }
        }
    }


}