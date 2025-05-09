using Microsoft.AspNetCore.Mvc;
using MovieTicket_Backend.Models;
using MovieTicket_Backend.Repositories;
using MovieTicket_Backend.RepositoryImpl;

namespace MovieTicket_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeatReservationController : ControllerBase
    {
        private readonly ISeatReservationRepository _seatReservationRepository;

        public SeatReservationController(ISeatReservationRepository seatReservationRepository)
        {
            _seatReservationRepository = seatReservationRepository;
        }

        [HttpPost("reserve")]
        public async Task<IActionResult> ReserveSeat(ReserveSeatRequest request)
        {
            var (success, message) = await _seatReservationRepository.ReserveSeatAsync(request);

            if (success)
            {
                return Ok(new { success, message });
            }

            return BadRequest(new { success, message });
        }

        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmReservation(ReserveSeatRequest request)
        {
            var success = await _seatReservationRepository.ConfirmReservationAsync(
                request.ShowingId, request.SeatId, request.UserId);

            if (success)
            {
                return Ok(new { success, message = "Đặt ghế thành công" });
            }

            return BadRequest(new { success, message = "Không thể xác nhận đặt ghế" });
        }

        [HttpPost("cancel")]
        public async Task<IActionResult> CancelReservation(ReserveSeatRequest request)
        {
            var success = await _seatReservationRepository.CancelReservationAsync(
                request.ShowingId, request.SeatId, request.UserId);

            if (success)
            {
                return Ok(new { success, message = "Đã hủy đặt ghế" });
            }

            return BadRequest(new { success, message = "Không thể hủy đặt ghế" });
        }
    }
}
