using StackExchange.Redis;
using System.Security.Cryptography;

namespace MovieTicket_Backend.Services
{
    public class VerificationService : IVerificationService
    {
        private readonly IDatabase _redis;
        private const int ExpirationMinutes = 10;

        public VerificationService(IConnectionMultiplexer redis)
        {
            _redis = redis.GetDatabase();
        }

        public string GenerateVerificationCode()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            int code = BitConverter.ToInt32(bytes, 0) % 1000000;
            code = Math.Abs(code);
            return code.ToString("D6");
        }

        public async Task<bool> StoreVerifyCodeAsync(string email, string code)
        {
            var result = await _redis.StringSetAsync(email, code, TimeSpan.FromMinutes(ExpirationMinutes));
            return result;
        }

        public async Task<bool> VerifyCodeAsync(string email, string code)
        {
            var storedCode = await _redis.StringGetAsync(email);
            if (storedCode.IsNullOrEmpty)
            {
                return false; // Code not found or expired  
            }
            if (storedCode == code)
            {
                await _redis.KeyDeleteAsync(email); // Delete the code after successful verification  
                return true;
            }
            return false; // Code does not match  
        }

        public async Task DeleteVerifyCodeAsync(string email)
        {
            var result = await _redis.KeyDeleteAsync(email);
        }
    }
}
