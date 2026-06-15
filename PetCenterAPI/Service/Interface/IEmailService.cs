namespace PetCenterAPI.Service.Interface
{
    public interface IEmailService
    {
        // ============================================================
        // REGISTER — OTP
        // ============================================================
        Task SendVerificationEmail(string toEmail, string code);

        // ============================================================
        // GOOGLE LOGIN
        // ============================================================
        Task<bool> SendWelcomeEmailAsync(string toEmail, string fullName, string tempPassword);
        Task<bool> SendResetPasswordEmailAsync(string toEmail, string fullName, string resetLink);
    }
}
