using Newtonsoft.Json;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;
using System.Net.Http.Headers;
using System.Text;

namespace PetCenterClient.Services
{
    public class AdminFeedbackService : IAdminFeedbackService
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdminFeedbackService(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        // ── Attach JWT token ──────────────────────────────────
        private void AttachToken()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT") ?? "";
            _http.DefaultRequestHeaders.Authorization = null;
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        // ── GET list with filter + paging ─────────────────────
        public async Task<FeedbackPagedResult?> GetAllAsync(
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

                var url = $"feedback-service/AdminProductFeedback/list?{string.Join("&", query)}";
                var response = await _http.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<AdminFeedbackApiResponse<FeedbackPagedResult>>(content);
                return result?.Data;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        // ── GET single feedback by id ─────────────────────────
        public async Task<AdminFeedbackItemDto?> GetByIdAsync(Guid feedbackId)
        {
            try
            {
                AttachToken();
                var response = await _http.GetAsync(
                    $"feedback-service/AdminProductFeedback/{feedbackId}");

                if (!response.IsSuccessStatusCode) return null;

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<AdminFeedbackApiResponse<AdminFeedbackItemDto>>(content);
                return result?.Data;
            }
            catch { return null; }
        }

        // ── POST reply ────────────────────────────────────────
        public async Task<(bool success, string message)> ReplyAsync(ReplyFeedbackDto dto)
        {
            try
            {
                AttachToken();
                var json = JsonConvert.SerializeObject(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _http.PostAsync(
                    "feedback-service/AdminProductFeedback/reply", content);

                var body = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<AdminFeedbackApiResponse<bool>>(body);

                return (
                    response.IsSuccessStatusCode,
                    result?.Message ?? (response.IsSuccessStatusCode ? "Reply thành công." : "Có lỗi xảy ra.")
                );
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        // ── PUT update reply ──────────────────────────────────
        public async Task<(bool success, string message)> UpdateReplyAsync(UpdateReplyDto dto)
        {
            try
            {
                AttachToken();
                var json = JsonConvert.SerializeObject(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // ← Backend API dùng PUT, gọi đúng PUT tới gateway
                var response = await _http.PutAsync(
                    "feedback-service/AdminProductFeedback/reply", content);

                var body = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<AdminFeedbackApiResponse<bool>>(body);

                return (
                    response.IsSuccessStatusCode,
                    result?.Message ?? (response.IsSuccessStatusCode ? "Cập nhật thành công." : "Có lỗi xảy ra.")
                );
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        // ── DELETE reply ──────────────────────────────────────
        public async Task<(bool success, string message)> DeleteReplyAsync(Guid feedbackId)
        {
            try
            {
                AttachToken();
                var response = await _http.DeleteAsync(
                    $"feedback-service/AdminProductFeedback/reply/{feedbackId}");

                var body = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<AdminFeedbackApiResponse<bool>>(body);

                return (
                    response.IsSuccessStatusCode,
                    result?.Message ?? (response.IsSuccessStatusCode ? "Đã xóa reply." : "Có lỗi xảy ra.")
                );
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        // ── PATCH toggle visibility ───────────────────────────
        public async Task<(bool success, string message)> ToggleVisibilityAsync(Guid feedbackId, bool isVisible)
        {
            try
            {
                AttachToken();
                var response = await _http.PatchAsync(
                    $"feedback-service/AdminProductFeedback/visibility?feedbackId={feedbackId}&isVisible={isVisible.ToString().ToLower()}",
                    null);

                var body = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<AdminFeedbackApiResponse<bool>>(body);

                return (
                    response.IsSuccessStatusCode,
                    result?.Message ?? (response.IsSuccessStatusCode ? "Cập nhật thành công." : "Có lỗi xảy ra.")
                );
            }
            catch (Exception ex) { return (false, ex.Message); }
        }
    }
}
