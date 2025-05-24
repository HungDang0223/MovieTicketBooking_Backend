using Microsoft.AspNetCore.Mvc;
using MovieTicket_Backend.Models;
using MovieTicket_Backend.Repositories;
using MovieTicket_Backend.Services;

namespace MovieTicket_Backend.Controllers
{
    [ApiController]
    [Route("api/seat-reserve")]
    public class SeatReservationController : ControllerBase
    {
        private readonly ISeatReservationRepository _seatReservationRepository;
        private readonly ISeatReservationNotificationService _notificationService;

        public SeatReservationController(
            ISeatReservationRepository seatReservationRepository,
            ISeatReservationNotificationService notificationService)
        {
            _seatReservationRepository = seatReservationRepository;
            _notificationService = notificationService;
        }

        [HttpPost("reserve")]
        public async Task<IActionResult> ReserveSeat(ReserveSeatRequest request)
        {
            var (success, message) = await _seatReservationRepository.ReserveSeatAsync(request);

            if (success)
            {
                // Notify WebSocket clients about seat status change
                var update = new SeatStatusUpdate
                {
                    SeatId = request.SeatId,
                    Status = SeatStatus.Reserved,
                    ReservedBy = int.TryParse(request.UserId, out var userId) ? userId : null,
                    ReservationExpiresAt = DateTime.Now.AddMinutes(10)
                };

                await _notificationService.NotifySeatStatusChangeAsync(request.ShowingId, update);

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
                // Notify WebSocket clients about seat confirmation
                var update = new SeatStatusUpdate
                {
                    SeatId = request.SeatId,
                    Status = SeatStatus.Sold,
                    ReservedBy = int.TryParse(request.UserId, out var userId) ? userId : null,
                    ReservationExpiresAt = null
                };

                await _notificationService.NotifySeatStatusChangeAsync(request.ShowingId, update);

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
                // Notify WebSocket clients about seat cancellation
                var update = new SeatStatusUpdate
                {
                    SeatId = request.SeatId,
                    Status = SeatStatus.Available,
                    ReservedBy = null,
                    ReservationExpiresAt = null
                };

                await _notificationService.NotifySeatStatusChangeAsync(request.ShowingId, update);

                return Ok(new { success, message = "Đã hủy đặt ghế" });
            }

            return BadRequest(new { success, message = "Không thể hủy đặt ghế" });
        }
    }
}