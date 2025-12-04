using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using WEBPC_API.Helpers;
using WEBPC_API.Services.Interfaces;

namespace WEBPC_API.Services.Business
{
    public class MailService : IMailService
    {
        private readonly MailSettings _mailSettings;

        public MailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var email = new MimeMessage();

                // Người gửi
                email.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail));

                // Người nhận (Đây là chỗ xử lý "không code cứng người nhận")
                email.To.Add(MailboxAddress.Parse(toEmail));

                // Nội dung
                email.Subject = subject;

                var builder = new BodyBuilder();
                builder.HtmlBody = body; // Hỗ trợ HTML để email đẹp hơn
                email.Body = builder.ToMessageBody();

                // Gửi mail qua SMTP
                using var smtp = new SmtpClient();

                // Kết nối tới Server Gmail
                await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);

                // Đăng nhập
                await smtp.AuthenticateAsync(_mailSettings.Mail, _mailSettings.Password);

                // Gửi
                await smtp.SendAsync(email);

                // Ngắt kết nối
                await smtp.DisconnectAsync(true);

                return true;
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu cần (Console.WriteLine(ex.Message));
                return false;
            }
        }
    }
}