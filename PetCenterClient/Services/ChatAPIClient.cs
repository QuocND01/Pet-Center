using System.Net.Http.Headers;
using PetCenterClient.Services.Interface;
using PetCenterClient.ViewModels;

namespace PetCenterClient.Services
{
    public class ChatAPIClient : IChatAPIClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChatAPIClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        private void AddAuthorizationHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT");
            if (!string.IsNullOrEmpty(token))
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<List<ChatCustomerViewModel>> GetMyCustomersAsync()
        {
            AddAuthorizationHeader();
            var res = await _http.GetAsync("api/chat/my-customers");
            if (res.IsSuccessStatusCode)
                return await res.Content.ReadFromJsonAsync<List<ChatCustomerViewModel>>() ?? new List<ChatCustomerViewModel>();

            return new List<ChatCustomerViewModel>();
        }

        public async Task<List<ChatMessageViewModel>> GetChatHistoryAsync(Guid partnerId)
        {
            AddAuthorizationHeader();
            var res = await _http.GetAsync($"api/chat/history/{partnerId}");
            if (res.IsSuccessStatusCode)
                return await res.Content.ReadFromJsonAsync<List<ChatMessageViewModel>>() ?? new List<ChatMessageViewModel>();

            return new List<ChatMessageViewModel>();
        }
    }
}