namespace MovieTicket_Backend.Services
{
    public interface IEmailService
    {
        Task SendVerifyEmailAsync(String email);
    }
}
