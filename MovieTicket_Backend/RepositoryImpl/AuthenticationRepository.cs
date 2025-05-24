using Dapper;
using Microsoft.IdentityModel.Tokens;
using MovieTicket_Backend.Data;
using MovieTicket_Backend.Models;
using MovieTicket_Backend.Repositories;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MovieTicket_Backend.RepositoryImpl
{
    public class AuthenticationRepository : IAuthenticationRepository
    {
        private readonly DbConnectionFactory _dbConnectionFactory;
        private readonly IConfiguration _configuration;
        private readonly MySqlConnection connection;

        public AuthenticationRepository(DbConnectionFactory dbConnectionFactory, IConfiguration configuration)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _configuration = configuration;
        }
        public string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("PhoneNumber", user.PhoneNumber)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }
            return Convert.ToBase64String(randomNumber);
        }

        // Save Refresh Token in Database
        public async Task SaveRefreshToken(string userId, string refreshToken)
        {
            var expiryDate = DateTime.UtcNow.AddDays(7);

            using var connection = _dbConnectionFactory.CreateConnection();

            var sql = "UPDATE user SET refresh_token = @RefreshToken, refresh_token_expiry = @Expiry WHERE user_id = @UserId";

            Debugger.Log(1, $"Executing SQL: {sql}", $"Params: UserId={userId}, RefreshToken={refreshToken}, Expiry={expiryDate}");

            int affectedRows = await connection.ExecuteAsync(sql, new { UserId = userId, RefreshToken = refreshToken, Expiry = expiryDate });

            if (affectedRows == 0)
            {
                throw new Exception("No rows were updated.");
            }
        }

        public async Task<bool> ValidateRefreshToken(string userId, string refreshToken)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            var sql = @"SELECT * FROM user WHERE user_id = @UserId AND refresh_token = @RefreshToken";
            var user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { UserId = userId, RefreshToken = refreshToken });
            return user != null;
        }

        // Get UserId from token
        public string? GetUserIdFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var claims = jwtToken.Claims;
            var userIdClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim != null)
            {
                return userIdClaim.Value;
            }
            return null;
        }
    }
}
