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

        public async Task<bool> SendWelcomeEmailAsync(string toEmail, string fullName, string tempPassword)
        {
            try
            {
                var subject = "Welcome to PetCenter! 🐾";
                var body = $@"
            <html>
            <body style='font-family:Arial,sans-serif;background:#f5f5f5;'>
              <div style='max-width:600px;margin:0 auto;background:white;
                          padding:30px;border-radius:10px;'>
                <h2 style='color:#2c3e50;border-bottom:3px solid #4285F4;
                            padding-bottom:15px;'>
                  Welcome to PetCenter, {fullName}!
                </h2>

                <p style='color:#34495e;'>
                  Your account has been created via <strong>Google Sign-In</strong>.
                </p>

                <div style='background:#ecf0f1;padding:15px;
                            border-left:4px solid #4285F4;margin:20px 0;'>
                  <p><strong>Email:</strong> {toEmail}</p>
                  <p><strong>Temporary Password:</strong>
                    <span style='font-family:monospace;color:#e74c3c;
                                 font-size:16px;'>{tempPassword}</span>
                  </p>
                </div>

                <div style='background:#fff3cd;padding:15px;
                            border-left:4px solid #ffc107;margin:20px 0;'>
                  <strong>Security notes:</strong>
                  <ul>
                    <li>You can login with <strong>email + password</strong>
                        OR continue using <strong>Google Sign-In</strong></li>
                    <li>We recommend changing this temporary password after login</li>
                  </ul>
                </div>

                <div style='background:#e8f5e9;padding:15px;
                            border-left:4px solid #27ae60;margin:20px 0;'>
                  <strong>Please complete your profile:</strong>
                  <ul>
                    <li>Phone number (required for orders)</li>
                    <li>Delivery address (required for orders)</li>
                  </ul>
                </div>

                <p style='color:#7f8c8d;font-size:13px;margin-top:30px;'>
                  © 2025 PetCenter. All rights reserved.
                </p>
              </div>
            </body>
            </html>";

                using var client = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587);
                client.EnableSsl = true;
                client.Credentials = new System.Net.NetworkCredential(
                    _configuration["EmailSettings:Username"],
                    _configuration["EmailSettings:Password"]);

                var mail = new System.Net.Mail.MailMessage
                {
                    From = new System.Net.Mail.MailAddress(
                        _configuration["EmailSettings:Username"]!, "PetCenter"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mail.To.Add(toEmail);
                await client.SendMailAsync(mail);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SendWelcomeEmail error: {ex.Message}");
                return false;
            }
        }
    }
}
