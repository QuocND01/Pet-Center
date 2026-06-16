namespace PetCenterAPI.Service.Interface
{
    public interface IForgotPasswordService
    {
        // ============================================================
        // FORGOT PASSWORD
        // ============================================================
        Task<(bool Success, string Message)> SendResetPasswordEmailAsync(string email);
        Task<(bool Valid, string Message)> ValidateResetTokenAsync(string email, string token);
        Task<(bool Success, string Message)> ResetPasswordAsync(
            string email, string token, string newPassword);
    }
}
