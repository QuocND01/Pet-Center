namespace IdentityAPI.Service.Interface
{
    public interface IEmailService
    {
        Task SendVerificationEmail(string toEmail, string code);
    }
}
