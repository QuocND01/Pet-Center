using Newtonsoft.Json;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;
using System.Text;

namespace PetCenterClient.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly HttpClient _httpClient;

        public FeedbackService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://localhost:7116/api/");
        }

        public async Task<List<FeedbackDTO>> GetAllAsync()
        {
            var res = await _httpClient.GetAsync("feedback/admin/all");

            var json = await res.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<FeedbackDTO>>(json)
                   ?? new List<FeedbackDTO>();
        }

        public async Task<FeedbackDTO?> GetDetailAsync(Guid id)
        {
            var res = await _httpClient.GetAsync($"feedback/detail/{id}");

            var json = await res.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<FeedbackDTO>(json);
        }

        public async Task<List<FeedbackDTO>> GetByCustomerAsync(Guid customerId)
        {
            var res = await _httpClient.GetAsync($"feedback/customer/{customerId}");

            var json = await res.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<FeedbackDTO>>(json)
                   ?? new List<FeedbackDTO>();
        }

        public async Task<List<FeedbackDTO>> GetByProductAsync(Guid productId)
        {
            var res = await _httpClient.GetAsync($"feedback/product/{productId}");

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

            await _httpClient.PostAsync("feedback", content);
        }

        public async Task ReplyAsync(Guid feedbackId, Guid staffId, string reply)
        {
            var content = new StringContent(
                JsonConvert.SerializeObject(reply),
                Encoding.UTF8,
                "application/json");

            await _httpClient.PutAsync($"feedback/reply/{feedbackId}?staffId={staffId}", content);
        }

        public async Task UpdateReplyAsync(Guid feedbackId, string reply)
        {
            var content = new StringContent(
                JsonConvert.SerializeObject(reply),
                Encoding.UTF8,
                "application/json");

            await _httpClient.PutAsync($"feedback/reply/update/{feedbackId}", content);
        }

        public async Task DeleteReplyAsync(Guid feedbackId)
        {
            await _httpClient.DeleteAsync($"feedback/reply/{feedbackId}");
        }

        public async Task ToggleVisibilityAsync(Guid feedbackId)
        {
            await _httpClient.PutAsync($"feedback/admin/toggle-visibility/{feedbackId}", null);
        }

        public async Task DeleteAsync(Guid feedbackId)
        {
            await _httpClient.DeleteAsync($"feedback/{feedbackId}");
        }
    }
}