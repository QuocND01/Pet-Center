using Newtonsoft.Json;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;
using PetCenterClient.ViewModels.ManageFeedback;
using System.Net.Http.Headers;
using System.Text;

namespace PetCenterClient.Services
{
    public class AdminFeedbackApiService : IAdminFeedbackApiService
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdminFeedbackApiService(HttpClient http, IHttpContextAccessor httpContextAccessor)
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
        // FEEDBACK — VIEW LIST (ADMIN/STAFF)
        // ============================================================
        public async Task<FeedbackPagedResultViewModel?> GetAllAsync(
            int page = 1, int pageSize = 10,
            int? rating = null, bool? hasReply = null,
            string? keyword = null, string? sortBy = null)
        {
            try
            {
                AttachToken();

                var query = new List<string> { $"page={page}", $"pageSize={pageSize}" };

                if (rating.HasValue) query.Add($"rating={rating.Value}");
                if (hasReply.HasValue) query.Add($"hasReply={hasReply.Value.ToString().ToLower()}");
                if (!string.IsNullOrWhiteSpace(keyword))
                    query.Add($"keyword={Uri.EscapeDataString(keyword)}");
                if (!string.IsNullOrWhiteSpace(sortBy))
                    query.Add($"sortBy={sortBy}");

                var url = $"api/AdminFeedbacks/list?{string.Join("&", query)}";
                var response = await _http.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return null;

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<AdminFeedbackApiResponseViewModel<FeedbackPagedResultViewModel>>(content);

                return result?.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AdminFeedbackApiService] GetAllAsync error: {ex.Message}");
                return null;
            }
        }

        // ============================================================
        // FEEDBACK — VIEW DETAIL (ADMIN/STAFF)
        // ============================================================
        public async Task<AdminFeedbackItemViewModel?> GetByIdAsync(Guid feedbackId)
        {
            try
            {
                AttachToken();
                var response = await _http.GetAsync(
                    $"api/AdminFeedbacks/{feedbackId}");

                if (!response.IsSuccessStatusCode) return null;

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<AdminFeedbackApiResponseViewModel<AdminFeedbackItemViewModel>>(content);
                return result?.Data;
            }
            catch { return null; }
        }

        // ============================================================
        // FEEDBACK — REPLY
        // ============================================================
        public async Task<(bool success, string message)> ReplyAsync(ReplyFeedbackViewModel dto)
        {
            try
            {
                AttachToken();
                var json = JsonConvert.SerializeObject(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _http.PostAsync(
                    "api/AdminFeedbacks/reply", content);

                var body = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<AdminFeedbackApiResponseViewModel<bool>>(body);

                return (
                    response.IsSuccessStatusCode,
            result?.Message ?? (response.IsSuccessStatusCode
                ? "Reply submitted successfully."
                : "An error occurred.")
                );
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        // ============================================================
        // FEEDBACK — UPDATE REPLY
        // ============================================================
        public async Task<(bool success, string message)> UpdateReplyAsync(UpdateReplyViewModel dto)
        {
            try
            {
                AttachToken();
                var json = JsonConvert.SerializeObject(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _http.PutAsync(
                    "api/AdminFeedbacks/reply", content);

                var body = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<AdminFeedbackApiResponseViewModel<bool>>(body);

                return (
                    response.IsSuccessStatusCode,
            result?.Message ?? (response.IsSuccessStatusCode
                ? "Reply updated successfully."
                : "An error occurred.")
                );
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        // ============================================================
        // FEEDBACK — DELETE REPLY
        // ============================================================
        public async Task<(bool success, string message)> DeleteReplyAsync(Guid feedbackId)
        {
            try
            {
                AttachToken();
                var response = await _http.DeleteAsync(
                    $"api/AdminFeedbacks/reply/{feedbackId}");

                var body = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<AdminFeedbackApiResponseViewModel<bool>>(body);

                return (
                    response.IsSuccessStatusCode,
            result?.Message ?? (response.IsSuccessStatusCode
                ? "Reply deleted."
                : "An error occurred.")
                );
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        // ============================================================
        // FEEDBACK — TOGGLE VISIBILITY
        // ============================================================
        public async Task<(bool success, string message)> ToggleVisibilityAsync(Guid feedbackId, bool isVisible)
        {
            try
            {
                AttachToken();
                var response = await _http.PatchAsync(
                    $"api/AdminFeedbacks/visibility?feedbackId={feedbackId}&isVisible={isVisible.ToString().ToLower()}",
                    null);

                var body = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<AdminFeedbackApiResponseViewModel<bool>>(body);

                return (
                    response.IsSuccessStatusCode,
            result?.Message ?? (response.IsSuccessStatusCode
                ? "Update successful."
                : "An error occurred.")
                );
            }
            catch (Exception ex) { return (false, ex.Message); }
        }
    }
}
