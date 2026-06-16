using PetCenterClient.DTOs;
using PetCenterClient.ViewModels.Login;
using PetCenterClient.ViewModels.Register;

namespace PetCenterClient.Services.Interface
{
    public interface IAuthApiService
    {
        // ============================================================
        // LOGIN — CUSTOMER
        // ============================================================
        Task<LoginResponseViewModel?> LoginAsync(LoginViewModel dto);

        // ============================================================
        // LOGIN — STAFF / ADMIN
        // ============================================================
        Task<StaffLoginResponseViewModel?> StaffLoginAsync(StaffLoginViewModel dto);

        // ============================================================
        // REGISTER
        // ============================================================
        Task<(bool Success, string Message)> RegisterAsync(RegisterViewModel dto);

        // ============================================================
        // OTP
        // ============================================================
        Task<(bool Success, string Message)> VerifyOtpAsync(string email, string code);
        Task<(bool Success, string Message)> ResendOtpAsync(string email);

        Task<(bool Success, string Message)> ChangePasswordAsync(ChangePasswordRequestDto dto);

        // ============================================================
        // GOOGLE LOGIN
        // ============================================================
        Task<LoginResponseDto?> GoogleLoginAsync(string idToken);
        Task<LoginResponseViewModel?> GoogleCallbackAsync(string code, string redirectUri);

        // ============================================================
        // FORGOT PASSWORD
        // ============================================================
        Task<(bool Success, string Message)> ForgotPasswordAsync(string email);
        Task<ValidateTokenResponseViewModel> ValidateResetTokenAsync(string email, string token);
        Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordViewModel dto);
    }
}
