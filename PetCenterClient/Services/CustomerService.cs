using System.Net.Http.Headers;
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

        public async Task<CustomerProfileResponseDto?> GetProfileAsync()
        {
            try
            {
                // Lấy token từ session
                var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT");
                if (string.IsNullOrEmpty(token))
                    return null;

                // Clear previous headers
                _http.DefaultRequestHeaders.Authorization = null;

                // Add new Authorization header
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var response = await _http.GetAsync("api/customer/profile");

                if (!response.IsSuccessStatusCode)
                    return null;

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<CustomerProfileResponseDto>>();
                return result?.Data;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> UpdateProfileAsync(UpdateCustomerProfileRequestDto dto)
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT");
                if (string.IsNullOrEmpty(token))
                {
                    return false;
                }

                _http.DefaultRequestHeaders.Authorization = null;
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                // Create anonymous object with camelCase properties
                var data = new
                {
                    fullName = dto.FullName,
                    phoneNumber = dto.PhoneNumber,
                    birthDay = dto.BirthDay.ToString("yyyy-MM-dd"),
                    gender = dto.Gender
                };

                var jsonString = System.Text.Json.JsonSerializer.Serialize(data);

                var jsonContent = new StringContent(
                    jsonString,
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                // Use PutAsync instead of PutAsJsonAsync
                var response = await _http.PutAsync("api/customer/profile", jsonContent);


                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ UpdateProfile Error: {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                return false;
            }
        }
    }
    }

