using System.Net.Http.Headers;
using System.Text.Json;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CustomerService(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        private HttpRequestMessage CreateAuthorizedRequest(
            HttpMethod method, string url, HttpContent? content = null)
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT") ?? "";
            var request = new HttpRequestMessage(method, url);
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            if (content != null)
                request.Content = content;
            return request;
        }

        public async Task<CustomerProfileResponseDto?> GetProfileAsync()
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT");
                Console.WriteLine($"[DEBUG] Token: {(string.IsNullOrEmpty(token) ? "NULL/EMPTY" : token[..20] + "...")}");

                if (string.IsNullOrEmpty(token))
                    return null;

                var request = CreateAuthorizedRequest(HttpMethod.Get, "api/customer/profile");
                var response = await _http.SendAsync(request);

                Console.WriteLine($"[DEBUG] Status: {response.StatusCode}");

                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[DEBUG] Response body: {content}");

                if (!response.IsSuccessStatusCode)
                    return null;

                var result = await System.Text.Json.JsonSerializer.DeserializeAsync<ApiResponse<CustomerProfileResponseDto>>(
                    new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(content)),
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                Console.WriteLine($"[DEBUG] Result null: {result == null}, Data null: {result?.Data == null}");

                return result?.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<(bool Success, string Message)> UpdateProfileAsync(UpdateCustomerProfileRequestDto dto)
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT");
                if (string.IsNullOrEmpty(token))
                    return (false, "Unauthorized");

                _http.DefaultRequestHeaders.Authorization = null;
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var data = new
                {
                    fullName = dto.FullName,
                    phoneNumber = dto.PhoneNumber,
                    birthDay = dto.BirthDay.ToString("yyyy-MM-dd"),
                    gender = dto.Gender
                };

                var jsonContent = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(data),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                var response = await _http.PutAsync("api/customer/profile", jsonContent);
                var content = await response.Content.ReadAsStringAsync();
                var json = System.Text.Json.JsonDocument.Parse(content).RootElement;
                var message = json.TryGetProperty("message", out var msg) ? msg.GetString() ?? "" : "";

                return (response.IsSuccessStatusCode, message);
            }
            catch
            {
                return (false, "An error occurred. Please try again.");
            }
        }

        public async Task<List<CustomerListDto>> GetAllCustomersAsync()
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT");
                if (string.IsNullOrEmpty(token)) return new List<CustomerListDto>();

                _http.DefaultRequestHeaders.Authorization = null;
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _http.GetAsync("api/customers");
                if (!response.IsSuccessStatusCode) return new List<CustomerListDto>();

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<CustomerListDto>>>();
                return result?.Data ?? new List<CustomerListDto>();
            }
            catch { return new List<CustomerListDto>(); }
        }

        public async Task<CustomerDetailDto?> GetCustomerByIdAsync(Guid id)
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT");
                if (string.IsNullOrEmpty(token)) return null;

                _http.DefaultRequestHeaders.Authorization = null;
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _http.GetAsync($"api/customers/{id}");
                if (!response.IsSuccessStatusCode) return null;

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<CustomerDetailDto>>();
                return result?.Data;
            }
            catch { return null; }
        }

        public async Task<bool> ChangeCustomerStatusAsync(Guid customerId, bool isActive)
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT");
                if (string.IsNullOrEmpty(token))
                    return false;

                _http.DefaultRequestHeaders.Authorization = null;
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var data = new { isActive };
                var jsonString = System.Text.Json.JsonSerializer.Serialize(data);
                var jsonContent = new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");

                var response = await _http.PutAsync($"api/customers/{customerId}/status", jsonContent);

                if (!response.IsSuccessStatusCode)
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<string> GetDisplayNameAsync(Guid customerId)
        {
            try
            {
                // Gọi qua API Gateway giống các method khác
                var response = await _http.GetAsync(
                    $"api/customers/{customerId}/display-name");

                if (!response.IsSuccessStatusCode) return "Anonymous";

                var raw = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[DEBUG DisplayName] Body: {raw}");

                var json = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(raw);
                return json.TryGetProperty("displayName", out var name)
                    ? name.GetString() ?? "Anonymous"
                    : "Anonymous";
            }
            catch (Exception ex)
            {
                return "Anonymous";
            }
        }
    }
    }

