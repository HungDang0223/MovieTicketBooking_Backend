using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieTicket_Backend.Data;
using MovieTicket_Backend.Models;
using MovieTicket_Backend.Repositories;
using MovieTicket_Backend.RepositoryInpl;
using System.Globalization;
using static MovieTicket_Backend.ModelDTOs.ModelRequests;

namespace MovieTicket_Backend.Controllers
{
    [Route("api/v1/user")]
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly PasswordHasher<string> _passwordHasher;
        private readonly DbConnectionFactory _dbConnectionFactory;

        public UserController(IUserRepository userRepository, DbConnectionFactory dbConnectionFactory)
        {
            _userRepository = userRepository;
            _passwordHasher = new PasswordHasher<string>();
            _dbConnectionFactory = dbConnectionFactory;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userRepository.GetAllUsers();
            return Ok(users);
        }

        // GET: user/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(string id)
        {
            var user = await _userRepository.GetUserById(id);
            if (user == null)
            {
                return NotFound(new { status = "error", message = "User not found" });
            }
            return Ok(user);
        }

        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] User user)
        {
            var result = await _userRepository.UpdateUserInfo(user);
            if (result == false)
            {
                return NotFound(new { status = "error", message = "User not found or something went error" });
            }
            return Ok(new { status = "success", message = "User updated successfully" });
        }

        // update password
        [HttpPost("update-password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request)
        {
            var user = await _userRepository.GetUserByEmailPhone(request.emailPhone);
            if (user == null)
            {
                return NotFound(new { status = "error", message = "User not found" });
            }

            request.newPassword = _passwordHasher.HashPassword(user.UserId, request.newPassword);
            await _userRepository.UpdateUserPassword(request);
            return Ok(new { status = "success", message = "Password updated successfully" });
        }

        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] string userId)
        {
            var result = await _userRepository.DeleteUser(userId);
            if (result == false)
            {
                return NotFound(new { status = "error", message = "User not found or something went error" });
            }
            return Ok(new { status = "success", message = "User deleted successfully" });
        }

        [HttpGet("{userId}/tickets")]
        public async Task<IActionResult> GetUserBookings(string userId)
        {
            var user = await _userRepository.GetUserById(userId);
            if (user == null)
            {
                return NotFound(new { status = "error", message = "User not found" });
            }

            var tickets = await _userRepository.GetUserBookings(userId);
            if (tickets == null || !tickets.Any())
            {
                return Ok(new { user_id = userId, count = 0, ticket = new List<object>() });
            }

            var bookingDetails = tickets.Select(ticket => new
            {
                id = ticket.BookingId,
                ticket.BookingDate,
                ticket.BookingAmount,
                seats = ticket.BookingSeats.Select(ticket => new
                {
                    ticket.Seat.SeatNumber
                }),
                snacks = ticket.BookingSnacks.Select(snack => new
                {
                    snack.Snack.SnackName,
                    snack.Quantity,
                    snack.Snack.Price
                }),
                combos = ticket.BookingCombos.Select(combo => new
                {
                    combo.Combo.ComboName,
                    combo.Quantity,
                    combo.Combo.Price
                })
            });

            return Ok(new
            {
                user_id = userId,
                count = tickets.Count,
                ticket = bookingDetails
            });
        }
    }
}
