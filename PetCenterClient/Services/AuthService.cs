using System.Text.Json;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
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

        // STEP 1: Gửi toàn bộ form → api/auth/register
        public async Task<(bool Success, string Message)> RegisterAsync(RegisterDto dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/auth/register", dto);
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content).RootElement;
                var message = json.TryGetProperty("message", out var msg) ? msg.GetString() ?? "" : "";

                return (response.IsSuccessStatusCode, message);
            }
            catch
            {
                return (false, "An error occurred. Please try again.");
            }
        }

        // STEP 2: Verify OTP → api/auth/verify-otp
        public async Task<(bool Success, string Message)> VerifyOtpAsync(string email, string code)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/auth/verify-otp", new { email, code });
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content).RootElement;
                var message = json.TryGetProperty("message", out var msg) ? msg.GetString() ?? "" : "";

                return (response.IsSuccessStatusCode, message);
            }
            catch
            {
                return (false, "An error occurred. Please try again.");
            }
        }

        // Resend OTP → api/auth/resend-otp
        public async Task<(bool Success, string Message)> ResendOtpAsync(string email)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/auth/resend-otp", new { email });
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content).RootElement;
                var message = json.TryGetProperty("message", out var msg) ? msg.GetString() ?? "" : "";

                return (response.IsSuccessStatusCode, message);
            }
            catch
            {
                return (false, "An error occurred. Please try again.");
            }
        }

        public async Task<(bool Success, string Message)> ChangePasswordAsync(ChangePasswordRequestDto dto)
        {
            try
            {
                // Lấy token từ IHttpContextAccessor
                // Cần inject IHttpContextAccessor vào AuthService
                var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT") ?? "";

                var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/change-password")
                {
                    Content = JsonContent.Create(dto)
                };
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _http.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content).RootElement;
                var message = json.TryGetProperty("message", out var msg) ? msg.GetString() ?? "" : "";

                return (response.IsSuccessStatusCode, message);
            }
            catch
            {
                return (false, "An error occurred. Please try again.");
            }
        }

        public async Task<LoginResponseDto?> GoogleLoginAsync(string idToken)
        {
            try
            {
                var response = await _http.PostAsJsonAsync(
                    "api/auth/google-login",
                    new GoogleLoginRequestDto { IdToken = idToken });

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var errorData = JsonDocument.Parse(errorContent).RootElement;

                    return new LoginResponseDto
                    {
                        Success = false,
                        message = errorData.TryGetProperty("message", out var msg) ? msg.GetString() : "Google login failed",
                        ErrorType = errorData.TryGetProperty("errorType", out var eType) ? eType.GetString() : "GoogleLoginFailed",
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

        // Thêm vào PetCenterClient/Services/AuthService.cs
        public async Task<LoginResponseDto?> GoogleCallbackAsync(string code, string redirectUri)
        {
            try
            {
                var response = await _http.PostAsJsonAsync(
                    "api/auth/google-callback",
                    new GoogleCallbackRequestDto { Code = code, RedirectUri = redirectUri });

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var errorData = JsonDocument.Parse(errorContent).RootElement;
                    return new LoginResponseDto
                    {
                        Success = false,
                        message = errorData.TryGetProperty("message", out var msg) ? msg.GetString() : "Google login failed",
                        ErrorType = errorData.TryGetProperty("errorType", out var eType) ? eType.GetString() : "GoogleLoginFailed",
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

        public async Task<(bool Success, string Message)> ForgotPasswordAsync(string email)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/auth/forgot-password", new { email });
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content).RootElement;
                var message = json.TryGetProperty("message", out var msg) ? msg.GetString() ?? "" : "";
                return (response.IsSuccessStatusCode, message);
            }
            catch
            {
                return (false, "An error occurred. Please try again.");
            }
        }

        public async Task<ValidateTokenResponseDto> ValidateResetTokenAsync(string email, string token)
        {
            try
            {
                var encodedEmail = Uri.EscapeDataString(email);
                var encodedToken = Uri.EscapeDataString(token);
                var response = await _http.GetAsync(
                    $"api/auth/validate-reset-token?email={encodedEmail}&token={encodedToken}");
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content).RootElement;
                return new ValidateTokenResponseDto
                {
                    Success = response.IsSuccessStatusCode,
                    Message = json.TryGetProperty("message", out var msg) ? msg.GetString() ?? "" : ""
                };
            }
            catch
            {
                return new ValidateTokenResponseDto { Success = false, Message = "An error occurred." };
            }
        }

        public async Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordRequestDto dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/auth/reset-password", dto);
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content).RootElement;
                var message = json.TryGetProperty("message", out var msg) ? msg.GetString() ?? "" : "";
                return (response.IsSuccessStatusCode, message);
            }
            catch
            {
                return (false, "An error occurred. Please try again.");
            }
        }
    }
}

