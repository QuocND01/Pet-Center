using Newtonsoft.Json;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;
using System.Text;

namespace PetCenterClient.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly HttpClient _httpClient;
        private const string PREFIX = "feedback-service/feedback/";
        public FeedbackService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<FeedbackDTO>> GetAllAsync()
        {
            var res = await _httpClient.GetAsync(PREFIX + "admin/all");
            var json = await res.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<FeedbackDTO>>(json)
                   ?? new List<FeedbackDTO>();
        }

        public async Task<FeedbackDTO?> GetDetailAsync(Guid id)
        {
            var res = await _httpClient.GetAsync(PREFIX + $"detail/{id}");
            var json = await res.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<FeedbackDTO>(json);
        }

        public async Task<List<FeedbackDTO>> GetByCustomerAsync(Guid customerId)
        {
            var res = await _httpClient.GetAsync(PREFIX + $"customer/{customerId}");
            var json = await res.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<FeedbackDTO>>(json)
                   ?? new List<FeedbackDTO>();
        }

        public async Task<List<FeedbackDTO>> GetByProductAsync(Guid productId)
        {
            var res = await _httpClient.GetAsync(PREFIX + $"product/{productId}");
            var json = await res.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<FeedbackDTO>>(json)
                   ?? new List<FeedbackDTO>();
        }

        public async Task CreateAsync(CreateFeedbackDTO dto)
        {
            var content = new StringContent(
                JsonConvert.SerializeObject(dto),
                Encoding.UTF8,
                "application/json");

            await _httpClient.PostAsync(PREFIX, content);
        }

        public async Task ReplyAsync(Guid feedbackId, Guid staffId, string reply)
        {
            var content = new StringContent(
                JsonConvert.SerializeObject(reply),
                Encoding.UTF8,
                "application/json");

            await _httpClient.PutAsync(PREFIX + $"reply/{feedbackId}?staffId={staffId}", content);
        }

        public async Task UpdateReplyAsync(Guid feedbackId, string reply)
        {
            var content = new StringContent(
                JsonConvert.SerializeObject(reply),
                Encoding.UTF8,
                "application/json");

            await _httpClient.PutAsync(PREFIX + $"reply/update/{feedbackId}", content);
        }

        public async Task DeleteReplyAsync(Guid feedbackId)
        {
            await _httpClient.DeleteAsync(PREFIX + $"reply/{feedbackId}");
        }

        public async Task ToggleVisibilityAsync(Guid feedbackId)
        {
            await _httpClient.PutAsync(PREFIX + $"admin/toggle-visibility/{feedbackId}", null);
        }

        public async Task DeleteAsync(Guid feedbackId)
        {
            await _httpClient.DeleteAsync(PREFIX + $"{feedbackId}");
        }

        public async Task<List<FeedbackDTO>> FilterAsync(
     int? rating,
     Guid? productId,
     bool? isVisible,
     DateTime? fromDate,
     DateTime? toDate)
        {
            var query = new List<string>();

            if (rating.HasValue)
                query.Add($"rating={rating}");

            if (productId.HasValue)
                query.Add($"productId={productId}");

            if (isVisible.HasValue)
                query.Add($"isVisible={isVisible}");

            if (fromDate.HasValue)
                query.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");

            if (toDate.HasValue)
                query.Add($"toDate={toDate.Value:yyyy-MM-dd}");

            var url = PREFIX + "admin/filter";

            if (query.Count > 0)
                url += "?" + string.Join("&", query);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return new List<FeedbackDTO>();

            var json = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<FeedbackDTO>>(json)
                   ?? new List<FeedbackDTO>();
        }
    }
}