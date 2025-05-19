using MimeKit;
using MailKit.Net.Smtp;
using StackExchange.Redis;
using MovieTicket_Backend.Models.ModelRequests;

namespace MovieTicket_Backend.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly IVerificationService _verificationService;

        public EmailService(IConfiguration config, IConnectionMultiplexer redis)
        {
            _config = config;
            _verificationService = new VerificationService(redis);
        }

        public async Task SendVerifyEmailAsync(String email)
        {
            try
            {
                // 1. Tạo verification code và lưu trữ
                string verificationCode = await StoreVerificationCodeAsync(email);

                // 2. Gửi email chỉ khi lưu code thành công
                await SendEmailWithCodeAsync(email, verificationCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Complete process error: {ex.Message}");
                throw;
            }
        }

        private async Task<string> StoreVerificationCodeAsync(string email)
        {
            try
            {
                string verificationCode = _verificationService.GenerateVerificationCode();
                await _verificationService.StoreVerifyCodeAsync(email, verificationCode);
                return verificationCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Code storage error: {ex.Message}");
                throw;
            }
        }

        private async Task SendEmailWithCodeAsync(string email, string verificationCode)
        {
            try
            {
                var mimeMessage = new MimeMessage();
                mimeMessage.From.Add(MailboxAddress.Parse(_config["EmailSettings:SenderEmail"]));
                mimeMessage.To.Add(MailboxAddress.Parse(email));
                mimeMessage.Subject = "Đặt lại mật khẩu";

                string template = System.IO.File.ReadAllText("Utils/EmailBodyTemplates/ResetPasswordTemplate.html");
                string body = template.Replace("{{CODE}}", verificationCode);
                mimeMessage.Body = new TextPart("html") { Text = body };

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_config["EmailSettings:SmtpServer"],
                    int.Parse(_config["EmailSettings:SmtpPort"]),
                    MailKit.Security.SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_config["EmailSettings:SenderEmail"],
                    _config["EmailSettings:Password"]);
                await smtp.SendAsync(mimeMessage);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending error: {ex.Message}");
                throw;
            }
        }
    }
}
