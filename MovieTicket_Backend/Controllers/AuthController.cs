using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MovieTicket_Backend.Data;
using MovieTicket_Backend.Models;
using MovieTicket_Backend.Models.ModelRequests;
using MovieTicket_Backend.Repositories;
using MovieTicket_Backend.RepositoryInpl;
using MovieTicket_Backend.Services;
using StackExchange.Redis;
using System.Threading.Tasks;

namespace MovieTicket_Backend.Controllers
{
    [Route("api/v1/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthenticationRepository _authenticationRepository;
        private readonly PasswordHasher<string> _passwordHasher;
        private readonly DbConnectionFactory _dbConnectionFactory;
        private readonly IVerificationService _verificationService;

        public AuthController(IUserRepository userRepository, DbConnectionFactory dbConnectionFactory, IConnectionMultiplexer redis)
        {
            _userRepository = userRepository;
            _passwordHasher = new PasswordHasher<string>();
            _dbConnectionFactory = dbConnectionFactory;
            _verificationService = new VerificationService(redis);
        }

        // User Registration
        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingUserWithEmail = await _userRepository.GetUserByEmailPhone(request.Email);
            var existingUserWithPhone = await _userRepository.GetUserByEmailPhone(request.PhoneNumber);
            if (existingUserWithEmail != null || existingUserWithPhone != null)
            {
                return BadRequest(new { status = "error", message = "Email or Phone number already exists" });
            }

            string hashedPassword = _passwordHasher.HashPassword(null, request.Password);

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Password = hashedPassword,
                Gender = request.Gender,
                DateOfBirth = request.DateOfBirth,
                Address = request.Address,
                AccountStatus = "active"
            };

            var newUserId = await _userRepository.CreateUser(user);
            if (!string.IsNullOrEmpty(newUserId))
            {
                user.UserId = newUserId;
                return Ok(new
                {
                    status = "success",
                    message = "User registered successfully",
                    data = user
                });
            }
            else
            {
                return StatusCode(500, new
                {
                    status = "error",
                    message = "Something went wrong. Please try again later."
                });
            }
        }

        // User Login (Generate JWT)
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var (accessToken, refreshToken, user) = await _userRepository.AuthenticateUser(request.EmailOrPhone, request.Password);
            if (accessToken == null)
            {
                return Unauthorized(new { status = "error", message = "Invalid credentials." });
            }

            return Ok(new { accessToken, refreshToken, expiredIn = 30, user });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromForm] string refreshToken)
        {
            var connection = _dbConnectionFactory.CreateConnection();

            var sql = "SELECT * FROM user WHERE RefreshToken = @RefreshToken";
            var user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { RefreshToken = refreshToken });

            if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
                return Unauthorized(new { status = "error", message = "Invalid or expired refresh token" });

            var newAccessToken = _authenticationRepository.GenerateJwtToken(user);
            var newRefreshToken = _authenticationRepository.GenerateRefreshToken();

            // Update refresh token in the database
            await _authenticationRepository.SaveRefreshToken(user.UserId, newRefreshToken);

            return Ok(new { accessToken = newAccessToken, refreshToken = newRefreshToken });
        }
        // verify code
        [HttpPost("verify-code")]
        public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeRequest request)
        {
            var user = await _userRepository.GetUserByEmailPhone(request.EmailOrPhone);
            if (user == null)
            {
                return NotFound(new { status = "error", message = "User not found" });
            }
            var verifyResult = await _verificationService.VerifyCodeAsync(request.EmailOrPhone, request.Code);
            if (verifyResult)
            {
                return Ok(new { status = "success", message = "Verification successful" });
            }
            else
            {
                return BadRequest(new { status = "error", message = "Invalid verification code" });
            }
        }
    }
}
