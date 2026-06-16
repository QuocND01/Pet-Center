using Microsoft.AspNetCore.Identity.Data;
using PetCenterAPI.DTOs.Requests.CustomerProfile;
using PetCenterAPI.DTOs.Requests.Register;

namespace PetCenterAPI.Service.Interface
{
    public interface ICustomerAuthService
    {
        // ============================================================
        // LOGIN
        // ============================================================
        Task<(bool success, string? token, string? errorType, string message)> LoginAsync(
            string email, string password);

        // ============================================================
        // REGISTER
        // ============================================================
        Task<(bool Success, string Message)> RegisterAsync(RegisterRequestDTO request);

        // ============================================================
        // OTP
        // ============================================================
        Task<(bool Success, string Message)> VerifyOtpAsync(VerifyOtpRequestDTO request);
        Task<(bool Success, string Message)> ResendOtpAsync(string email);

        // ============================================================
        // CHANGE PASSWORD
        // ============================================================
        Task<(bool Success, string Message)> ChangePasswordAsync(
            Guid customerId, ChangePasswordRequestDTO request);
    }
}
