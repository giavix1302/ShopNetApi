using Microsoft.Extensions.Options;
using ShopNetApi.Settings;
using System.Net;
using System.Net.Mail;

namespace ShopNetApi.Services
{
    public class EmailService
    {
        private readonly SmtpSettings _smtp;

        public EmailService(IOptions<SmtpSettings> smtp)
        {
            _smtp = smtp.Value;
        }

        public async Task SendOtpAsync(string to, string otp)
        {
            var client = new SmtpClient(_smtp.Host, _smtp.Port)
            {
                Credentials = new NetworkCredential(
                    _smtp.Username,
                    _smtp.Password
                ),
                EnableSsl = true
            };

            var mail = new MailMessage(
                _smtp.From,
                to,
                "Xác thực đăng ký ShopNet",
                $"Mã OTP của bạn là: {otp}\nCó hiệu lực trong 5 phút."
            );

            await client.SendMailAsync(mail);
        }
    }
}
