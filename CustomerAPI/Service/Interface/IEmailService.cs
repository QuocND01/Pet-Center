namespace CustomerAPI.Service.Interface
{
    public interface IEmailService
    {
        Task SendVerificationEmail(string toEmail, string code);
        Task<bool> SendWelcomeEmailAsync(string toEmail, string fullName, string tempPassword);
        Task<bool> SendResetPasswordEmailAsync(string toEmail, string fullName, string resetLink);
    }
}
