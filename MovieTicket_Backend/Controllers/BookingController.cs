using Microsoft.AspNetCore.Mvc;
using MovieTicket_Backend.Data;
using MovieTicket_Backend.Models;
using MovieTicket_Backend.Models.ModelDTOs;
using MovieTicket_Backend.Repositories;
using MovieTicket_Backend.RepositoryInpl;
using static MovieTicket_Backend.Models.ModelDTOs.ModelRequests;

namespace MovieTicket_Backend.Controllers
{
    [Route("api/v1/")]
    [ApiController]
    public class BookingController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IBookingRepository _bookingRepository;

        public BookingController(IUserRepository userRepository, IBookingRepository bookingRepository)
        {
            _userRepository = userRepository;
            _bookingRepository = bookingRepository;
        }

        [HttpPost("book")]
        public async Task<IActionResult> BookTickets([FromBody] BookingRequestDTO bookingRequest)
        {
            if (bookingRequest == null || string.IsNullOrEmpty(bookingRequest.UserId))
            {
                return BadRequest(new { status = "error", message = "Invalid booking request" });
            }

            var user = await _userRepository.GetUserById(bookingRequest.UserId);
            if (user == null)
            {
                return NotFound(new { status = "error", message = "User not found" });
            }

            // Process the booking
            var bookingId = await _bookingRepository.CreateBooking(bookingRequest);
            if (string.IsNullOrEmpty(bookingId))
            {
                return StatusCode(500, new { status = "error", message = "Failed to create booking" });
            }

            return Ok(new { status = "success", message = "Booking created successfully", bookingId });
        }

        [HttpGet("{bookingId}/details")]
        public async Task<IActionResult> GetBookingDetails(int bookingId)
        {
            var bookingDetails = await _bookingRepository.GetBookingDetails(bookingId);

            if (bookingDetails == null)
            {
                return NotFound(new { message = "Booking not found" });
            }

            return Ok(new { bookingDetails });
        }
    }


}
