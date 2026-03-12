using IdentityAPI.DTOs.Resquest;

namespace IdentityAPI.Service.Interface
{
    public interface ICustomerAuthService
    {
        Task<(bool success, string token, string errorType, string message)> LoginAsync(string email, string password);

        Task<(bool Success, string Message)> RegisterAsync(RegisterDto dto);
        Task<(bool Success, string Message)> VerifyOtpAsync(VerifyOtpDto dto);
        Task<(bool Success, string Message)> ResendOtpAsync(string email);

    }
}
