using IdentityAPI.Service.Interface;
using System.Net.Mail;
using System.Net;

namespace IdentityAPI.Service
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendVerificationEmail(string toEmail, string code)
        {
            var username = _configuration["EmailSettings:Username"];
            var password = _configuration["EmailSettings:Password"];

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };

            var message = new MailMessage
            {
                From = new MailAddress(username),
                Subject = "PetCenter - Email Verification",
                Body = $"Your verification code is: {code}",
                IsBodyHtml = false
            };

            message.To.Add(toEmail);

            await smtpClient.SendMailAsync(message);
        }
    }
}
