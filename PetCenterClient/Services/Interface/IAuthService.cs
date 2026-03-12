using PetCenterClient.DTOs;

namespace PetCenterClient.Services.Interface
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginDto dto);

        Task<LoginResponseDto?> StaffLoginAsync(LoginDto dto);

        
        Task<(bool Success, string Message)> RegisterAsync(RegisterDto dto);

        
        Task<(bool Success, string Message)> VerifyOtpAsync(string email, string code);

        
        Task<(bool Success, string Message)> ResendOtpAsync(string email);
    }
}
