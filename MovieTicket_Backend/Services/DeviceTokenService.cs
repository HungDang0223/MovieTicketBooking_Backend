using Microsoft.EntityFrameworkCore;
using MovieTicket_Backend.Data;
using MovieTicket_Backend.Models;

namespace MovieTicket_Backend.Services
{
    public interface IDeviceTokenService
    {
        Task<bool> RegisterDeviceToken(string userId, string token, string deviceType);
        Task<List<string>> GetUserTokens(string userId);
        Task<List<string>> GetTokensByUserIds(List<string> userIds);
    }

    public class DeviceTokenService : IDeviceTokenService
    {
        private readonly ApplicationDbContext _context;

        public DeviceTokenService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> RegisterDeviceToken(string userId, string token, string deviceType)
        {
            var existingToken = await _context.DeviceTokens
                .FirstOrDefaultAsync(t => t.Token == token);

            if (existingToken != null)
            {
                existingToken.UserId = userId;
                existingToken.DeviceType = deviceType;
                existingToken.LastUpdated = DateTime.UtcNow;
            }
            else
            {
                _context.DeviceTokens.Add(new DeviceToken
                {
                    Token = token,
                    UserId = userId,
                    DeviceType = deviceType,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<string>> GetUserTokens(string userId)
        {
            return await _context.DeviceTokens
                .Where(t => t.UserId == userId)
                .Select(t => t.Token)
                .ToListAsync();
        }

        public async Task<List<string>> GetTokensByUserIds(List<string> userIds)
        {
            return await _context.DeviceTokens
                .Where(t => userIds.Contains(t.UserId))
                .Select(t => t.Token)
                .ToListAsync();
        }
    }
}
