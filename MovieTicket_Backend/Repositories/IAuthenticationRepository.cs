using MovieTicket_Backend.Models;

namespace MovieTicket_Backend.Repositories
{
    public interface IAuthenticationRepository
    {
        public string GenerateJwtToken(User user);
        public string GenerateRefreshToken();
        public Task SaveRefreshToken(string userId, string refreshToken);
        public Task<bool> ValidateRefreshToken(string userId, string refreshToken);
        public string? GetUserIdFromToken(string token);
    }
}
