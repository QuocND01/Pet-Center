using System.Text.Json;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _http;

        public AuthService(HttpClient http)
        {
            _http = http;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/auth/customer-login", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var errorData = JsonDocument.Parse(errorContent).RootElement;

                    return new LoginResponseDto
                    {
                        Success = false,
                        message = errorData.GetProperty("message").GetString(),
                        ErrorType = errorData.GetProperty("errorType").GetString(),
                        token = null
                    };
                }

                return await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            }
            catch
            {
                return new LoginResponseDto
                {
                    Success = false,
                    message = "An error occurred. Please try again.",
                    ErrorType = "Exception",
                    token = null
                };
            }
        }

        public async Task<LoginResponseDto?> StaffLoginAsync(LoginDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/auth/staff-login", dto);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        }
    }
}

