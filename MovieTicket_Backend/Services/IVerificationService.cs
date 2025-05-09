namespace MovieTicket_Backend.Services
{
    internal interface IVerificationService
    {
        Task<bool> StoreVerifyCodeAsync(string email, string code);
        Task<bool> VerifyCodeAsync(string email, string code);
        Task DeleteVerifyCodeAsync(string email);
        string GenerateVerificationCode();
    }
}