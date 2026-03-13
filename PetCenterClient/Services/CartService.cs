using System.Net.Http.Headers;
using System.Text.Json;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Services
{
    public class CartService : ICartService
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string Route = "orders/Cart";

        public CartService(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        private void AddAuthHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT");
            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<CartResponseDTO?> GetCartAsync(Guid customerId)
        {
            try
            {
                AddAuthHeader();
                var response = await _http.GetAsync($"{Route}/{customerId}");
                if (!response.IsSuccessStatusCode) return null;

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<CartResponseDTO>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch
            {
                return null;
            }
        }

        public async Task<(bool Success, string Message)> AddToCartAsync(AddToCartRequestDTO dto)
        {
            try
            {
                AddAuthHeader();
                var response = await _http.PostAsJsonAsync($"{Route}/add", dto);
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(json);
                var message = result.TryGetProperty("message", out var msg) ? msg.GetString() ?? "" : "";
                return (response.IsSuccessStatusCode, message);
            }
            catch
            {
                return (false, "An error occurred. Please try again.");
            }
        }

        public async Task<(bool Success, string Message)> UpdateCartDetailAsync(Guid cartDetailId, int quantity)
        {
            try
            {
                AddAuthHeader();
                var response = await _http.PutAsJsonAsync($"{Route}/details/{cartDetailId}",
                    new { quantity });
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(json);
                var message = result.TryGetProperty("message", out var msg) ? msg.GetString() ?? "" : "";
                return (response.IsSuccessStatusCode, message);
            }
            catch
            {
                return (false, "An error occurred. Please try again.");
            }
        }

        public async Task<(bool Success, string Message)> DeleteCartDetailAsync(Guid cartDetailId)
        {
            try
            {
                AddAuthHeader();
                var response = await _http.DeleteAsync($"{Route}/details/{cartDetailId}");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(json);
                var message = result.TryGetProperty("message", out var msg) ? msg.GetString() ?? "" : "";
                return (response.IsSuccessStatusCode, message);
            }
            catch
            {
                return (false, "An error occurred. Please try again.");
            }
        }

        public async Task<(bool Success, string Message)> ClearCartAsync(Guid customerId)
        {
            try
            {
                AddAuthHeader();
                var response = await _http.DeleteAsync($"{Route}/clear/{customerId}");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(json);
                var message = result.TryGetProperty("message", out var msg) ? msg.GetString() ?? "" : "";
                return (response.IsSuccessStatusCode, message);
            }
            catch
            {
                return (false, "An error occurred. Please try again.");
            }
        }
    }
}