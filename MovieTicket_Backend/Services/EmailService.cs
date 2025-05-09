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

        public async Task SendVerifyEmailAsync(EmailRequest request)
        {
            var email = new MimeMessage();
            #region Config email infomation
            email.From.Add(MailboxAddress.Parse(_config["EmailSettings:SenderEmail"]));
            email.To.Add(MailboxAddress.Parse(request.To));
            email.Subject = "Đặt lại mật khẩu";
            string template = System.IO.File.ReadAllText("Utils/EmailBodyTemplates/ResetPasswordTemplate.html");
            string verificationCode = _verificationService.GenerateVerificationCode();
            await _verificationService.StoreVerifyCodeAsync(request.To, verificationCode);
            string body = template.Replace("{{CODE}}", verificationCode);
            email.Body = new TextPart("html") { Text = body };
            #endregion

            #region Send email
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_config["EmailSettings:SmtpServer"], int.Parse(_config["EmailSettings:SmtpPort"]), MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_config["EmailSettings:SenderEmail"], _config["EmailSettings:Password"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
            #endregion
        }
    }
}
