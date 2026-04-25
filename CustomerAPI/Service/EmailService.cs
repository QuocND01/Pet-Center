using CustomerAPI.Service.Interface;
using System.Net.Mail;
using System.Net;

namespace CustomerAPI.Service
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

        public async Task<bool> SendResetPasswordEmailAsync(string toEmail, string fullName, string resetLink)
        {
            try
            {
                var subject = "PetCenter - Reset Your Password 🔐";
                var body = $@"
<html>
<body style='font-family:Arial,sans-serif;background:#f5f5f5;margin:0;padding:0;'>
  <div style='max-width:600px;margin:0 auto;padding:40px 20px;'>
    <div style='background:white;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.08);'>
      <div style='background:linear-gradient(135deg,#2ecc71,#27ae60);padding:36px 40px;text-align:center;'>
        <div style='font-size:48px;margin-bottom:8px;'>🐾</div>
        <h1 style='color:white;margin:0;font-size:26px;font-weight:700;'>PetCenter</h1>
        <p style='color:rgba(255,255,255,0.85);margin:6px 0 0;font-size:14px;'>Password Reset Request</p>
      </div>
      <div style='padding:40px;'>
        <h2 style='color:#2c3e50;font-size:20px;margin:0 0 12px;'>Hi {fullName},</h2>
        <p style='color:#5a6a7a;font-size:15px;line-height:1.6;margin:0 0 24px;'>
          We received a request to reset the password for your PetCenter account.
          Click the button below to choose a new password.
        </p>
        <div style='text-align:center;margin:32px 0;'>
          <a href='{resetLink}'
             style='display:inline-block;background:linear-gradient(135deg,#2ecc71,#27ae60);
                    color:white;text-decoration:none;padding:16px 40px;border-radius:50px;
                    font-size:16px;font-weight:600;box-shadow:0 4px 16px rgba(46,204,113,0.4);'>
            Reset My Password
          </a>
        </div>
        <div style='background:#fff8e1;border:1px solid #ffe082;border-radius:10px;padding:16px 20px;margin:24px 0;'>
          <p style='margin:0;color:#b45309;font-size:13px;line-height:1.6;'>
            ⏰ <strong>This link expires in 15 minutes.</strong><br>
            If you did not request a password reset, you can safely ignore this email.
          </p>
        </div>
        <p style='color:#95a5a6;font-size:12px;margin:24px 0 0;'>
          If the button above doesn't work, copy and paste this URL into your browser:
        </p>
        <p style='color:#2ecc71;font-size:12px;word-break:break-all;background:#f0fdf4;
                  padding:10px 14px;border-radius:6px;margin:6px 0 0;'>
          {resetLink}
        </p>
      </div>
      <div style='background:#f8fafc;border-top:1px solid #e9ecef;padding:24px 40px;text-align:center;'>
        <p style='color:#adb5bd;font-size:12px;margin:0;'>
          © 2025 PetCenter. All rights reserved.<br>
          This is an automated message, please do not reply.
        </p>
      </div>
    </div>
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
                    From = new System.Net.Mail.MailAddress(_configuration["EmailSettings:Username"]!, "PetCenter"),
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
                Console.WriteLine($"SendResetPasswordEmail error: {ex.Message}");
                return false;
            }
        }
    }
}
