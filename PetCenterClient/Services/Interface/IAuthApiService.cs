using PetCenterClient.DTOs;
using PetCenterClient.ViewModels.Login;

namespace PetCenterClient.Services.Interface
{
    public interface IAuthApiService
    {
        Task<LoginResponseViewModel?> LoginAsync(LoginViewModel dto);

        Task<LoginStaffResponseDto?> StaffLoginAsync(LoginDto dto);

        
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
