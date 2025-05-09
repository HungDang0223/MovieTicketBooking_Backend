using MovieTicket_Backend.Models.ModelRequests;

namespace MovieTicket_Backend.Services
{
    public interface IEmailService
    {
        Task SendVerifyEmailAsync(EmailRequest request);
    }
}
