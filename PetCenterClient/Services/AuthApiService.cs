using System.Text.Json;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;
using PetCenterClient.ViewModels.Login;
using PetCenterClient.ViewModels.Register;

namespace PetCenterClient.Services
{
    public class AuthApiService : IAuthApiService
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthApiService(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        // ============================================================
        // LOGIN — CUSTOMER
        // ============================================================
        public async Task<LoginResponseViewModel?> LoginAsync(LoginViewModel dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/auths/customer-login", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var errorData = JsonDocument.Parse(errorContent).RootElement;

                    return new LoginResponseViewModel
                    {
                        Success = false,
                        Message = errorData.GetProperty("message").GetString(),
                        ErrorType = errorData.GetProperty("errorType").GetString(),
                        Token = null
                    };
                }

                return await response.Content.ReadFromJsonAsync<LoginResponseViewModel>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"[AuthApiService] LoginAsync error: {ex.Message}");
                
                return new LoginResponseViewModel
                {
                    Success = false,
                    Message = "An error occurred. Please try again.",
                    ErrorType = "Exception",
                    Token = null
                };
            }
        }

        // ============================================================
        // LOGIN — STAFF / ADMIN
        // ============================================================
        public async Task<StaffLoginResponseViewModel?> StaffLoginAsync(StaffLoginViewModel dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/auths/staff-login", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var errorData = JsonDocument.Parse(errorContent).RootElement;

                    return new StaffLoginResponseViewModel
                    {
                        Success = false,
                        Message = errorData.TryGetProperty("message", out var msg)
                            ? msg.GetString() : "Email or password incorrect",
                        ErrorType = errorData.TryGetProperty("errorType", out var eType)
                            ? eType.GetString() : "InvalidCredentials",
                        Token = null
                    };
                }

                return await response.Content.ReadFromJsonAsync<StaffLoginResponseViewModel>();
            }
            catch
            {
                return new StaffLoginResponseViewModel
                {
                    Success = false,
                    Message = "An error occurred. Please try again.",
                    ErrorType = "Exception",
                    Token = null
                };
            }
        }

        // ============================================================
        // REGISTER
        // ============================================================
        public async Task<(bool Success, string Message)> RegisterAsync(RegisterViewModel dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/auths/register", dto);
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

        // ============================================================
        // OTP — VERIFY
        // ============================================================
        public async Task<(bool Success, string Message)> VerifyOtpAsync(string email, string code)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/auths/verify-otp", new { email, code });
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

        // ============================================================
        // OTP — RESEND
        // ============================================================
        public async Task<(bool Success, string Message)> ResendOtpAsync(string email)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/auths/resend-otp", new { email });
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

