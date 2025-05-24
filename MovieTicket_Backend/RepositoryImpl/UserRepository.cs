using Dapper;
using Microsoft.AspNetCore.Connections;
using Microsoft.IdentityModel.Tokens;
using MovieTicket_Backend.Data;
using MovieTicket_Backend.ModelDTOs;
using MovieTicket_Backend.Models;
using MovieTicket_Backend.Repositories;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Crypto.Generators;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static MovieTicket_Backend.ModelDTOs.ModelRequests;

namespace MovieTicket_Backend.RepositoryInpl
{
    public class UserRepository : IUserRepository
    {
        private readonly DbConnectionFactory _dbConnectionFactory;
        private readonly IConfiguration _configuration;
        private readonly MySqlConnection connection;
        private readonly IAuthenticationRepository _authenticationRepository;

        public UserRepository(DbConnectionFactory dbConnectionFactory, IConfiguration configuration, IAuthenticationRepository authenticationRepository)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _configuration = configuration;
            _authenticationRepository = authenticationRepository;
        }

        public async Task<List<User?>> GetAllUsers()
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            var listUser = (await connection.QueryAsync<User>("select * from user")).ToList();
            return listUser;
        }

        public async Task<User?> GetUserById(string userId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            var user = await connection.QueryFirstOrDefaultAsync<User>("SELECT * FROM user WHERE user_id = @UserId", new { UserId = userId });
            return user;
        }

        public async Task<User?> GetUserByEmailPhone(string emailPhone)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            var user = await connection.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM user WHERE email = @Email or phone_number = @PhoneNumber", new { Email = emailPhone, PhoneNumber = emailPhone });
            return user;
        }

        public async Task<string?> CreateUser(UserDTO user)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            try
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

                string query = @"
                        INSERT INTO user (full_name, email, phone_number, password, gender, date_of_birth, address)
                        VALUES (@FullName, @Email, @PhoneNumber, @Password, @Gender, @DateOfBirth, @Address);
                        SELECT LAST_INSERT_ID();";

                var userId = await connection.ExecuteScalarAsync<string>(query, user);
                return userId;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Database error creating user: {ex.Message}");
                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error creating user: {ex.Message}");
                return string.Empty;
            }
        }

        public async Task<bool> UpdateUserInfo(User user)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            string query = @"
                            UPDATE user 
                            SET full_name = @FullName, phone_number = @PhoneNumber, date_of_birth = @DateOfBirth, 
                                address = @Address, account_status = @AccountStatus
                            WHERE user_id = @UserId";

            int rowsAffected = await connection.ExecuteAsync(query, user);
            return rowsAffected > 0;
        }

        public async Task<bool> UpdatePassword(UpdatePasswordRequest request)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            string query = @"
                            UPDATE user 
                            SET password = @Password
                            WHERE phone_number = @EmailPhone OR email = @EmailPhone";

            int rowsAffected = await connection.ExecuteAsync(query, new { EmailPhone = request.emailPhone});
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteUser(string userId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            var query = @"
                        UPDATE users 
                        SET email = CONCAT(email, '__deleted),
                            phone_number = CONCAT(phone_number, '__deleted)
                            account_status = 'Deleted' WHERE user_id=@userId";
            int rowsAffected = await connection.ExecuteAsync(query, new { userId });
            return rowsAffected > 0;
        }

        public async Task<(string AccessToken, string RefreshToken, User user)> AuthenticateUser(string emailPhone, string password)
        {
            var user = await GetUserByEmailPhone(emailPhone);
            //  || !BCrypt.Net.BCrypt.Verify(password, user.Password)
            if (user == null || password != user.Password)
            {
                return (null, null, user);
            }

            string accessToken = _authenticationRepository.GenerateJwtToken(user);
            string refreshToken = _authenticationRepository.GenerateRefreshToken();

            await _authenticationRepository.SaveRefreshToken(user.UserId, refreshToken);

            return (accessToken, refreshToken, user);
        }

        public async Task<List<Ticket>> GetUserBookings(string userId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            var sql = @"SELECT * FROM booking WHERE user_id = @UserId";
            var bookings = await connection.QueryAsync<Ticket>(sql, new { UserId = userId });
            return bookings.ToList();
        }

        public async Task<bool> UpdateUserPassword(UpdatePasswordRequest request)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            string query = @"
                            UPDATE user 
                            SET password = @Password
                            WHERE phone_number = @EmailPhone OR email = @EmailPhone";
            int rowsAffected = await connection.ExecuteAsync(query, new { EmailPhone = request.emailPhone, Password = request.newPassword });
            return rowsAffected > 0;
        }
    }
}
