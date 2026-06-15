using PetCenterClient.DTOs;
using PetCenterClient.ViewModels.Login;

namespace PetCenterClient.Services.Interface
{
    public interface IAuthApiService
    {
        // ============================================================
        // LOGIN — CUSTOMER
        // ============================================================

        /// <summary>
        /// Send customer login request to backend and return token or error
        /// </summary>
        Task<LoginResponseViewModel?> LoginAsync(LoginViewModel dto);

        // ============================================================
        // LOGIN — STAFF / ADMIN
        // ============================================================

        /// <summary>
        /// Send staff or admin login request to backend and return token, roles or error
        /// </summary>
        Task<StaffLoginResponseViewModel?> StaffLoginAsync(StaffLoginViewModel dto);

        
        Task<(bool Success, string Message)> RegisterAsync(RegisterDto dto);

        
        Task<(bool Success, string Message)> VerifyOtpAsync(string email, string code);

        
        Task<(bool Success, string Message)> ResendOtpAsync(string email);

        Task<(bool Success, string Message)> ChangePasswordAsync(ChangePasswordRequestDto dto);
        Task<LoginResponseDto?> GoogleLoginAsync(string idToken);

        Task<LoginResponseDto?> GoogleCallbackAsync(string code, string redirectUri);

        Task<(bool Success, string Message)> ForgotPasswordAsync(string email);
        Task<ValidateTokenResponseDto> ValidateResetTokenAsync(string email, string token);
        Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordRequestDto dto);
    }
}
